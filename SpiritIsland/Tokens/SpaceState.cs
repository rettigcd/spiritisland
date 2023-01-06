namespace SpiritIsland;

/// <summary>
/// Wraps: Space, Token-Counts on that space, API to publish token-changed events.
/// </summary>
public class SpaceState : HasNeighbors<SpaceState> {

	#region constructor

	public SpaceState( Space space, CountDictionary<Token> counts, IIslandTokenApi tokenApi, GameState gameState ) {
		Space = space;
		_counts = counts;
		_api = tokenApi;
		_gameState = gameState;
	}

	/// <summary> Clone / copy constructor </summary>
	public SpaceState( SpaceState src ) {
		this.Space = src.Space;
		_counts = src._counts.Clone();
		_api = src._api;
	}

	#endregion

	public Space Space { get; }

	public BoardState Board => new BoardState( Space.Board, _gameState );

	public int this[Token specific] {
		get {
			ValidateNotDead( specific );
			int count = _counts[specific];
			if( specific is UniqueToken ut )
				count += _api.GetDynamicTokensFor(this, ut);
			return count;
		}
		private set {
			ValidateNotDead( specific );
			_counts[specific] = value; 
		}
	}

	public IEnumerable<Token> Keys => _counts.Keys; // !! This won't list virtual (defend) tokens

	#region Enumeration / Detection(HaS) / Summing

	protected IEnumerable<Token> OfCategoryInternal( TokenCategory category ) => Keys.Where( k => k.Class.Category == category );
	protected IEnumerable<Token> OfClassInternal( TokenClass tokenClass ) => Keys.Where( x => x.Class == tokenClass );
	protected IEnumerable<Token> OfAnyClassInternal( params TokenClass[] classes ) => Keys.Where( specific => classes.Contains( specific.Class ) );

	public Token[] OfCategory( TokenCategory category ) => OfCategoryInternal( category ).ToArray();
	public Token[] OfClass( TokenClass tokenClass ) => OfClassInternal( tokenClass ).ToArray();
	public Token[] OfAnyClass( params TokenClass[] classes ) => OfAnyClassInternal( classes ).ToArray();
	public HealthToken[] OfHealthClass( HealthTokenClass tokenClass ) => OfClassInternal( tokenClass ).Cast<HealthToken>().ToArray();
	public HealthToken[] OfAnyHealthClass( params HealthTokenClass[] classes ) => OfAnyClassInternal( classes ).Cast<HealthToken>().ToArray();

	public bool Has( TokenClass inv ) => OfClassInternal( inv ).Any();
	public bool Has( TokenCategory cat ) => OfCategoryInternal( cat ).Any();
	public bool HasAny( params TokenClass[] healthyInvaders ) => OfAnyClassInternal( healthyInvaders ).Any();

	public int Sum( TokenClass tokenClass ) => OfClassInternal( tokenClass ).Sum( k => _counts[k] );
	public int Sum( TokenCategory category ) => OfCategoryInternal( category ).Sum( k => _counts[k] );
	public int SumAny( params TokenClass[] healthyInvaders ) => OfAnyClassInternal( healthyInvaders ).Sum( k => _counts[k] );

	#endregion

	#region To-String methods

	/// <summary>Gets all tokens that have a SpaceAbreviation</summary>
	public string Summary => _counts.TokenSummary();

	public override string ToString() => Space.Label + ":" + Summary;

	#endregion

	#region Token-Type Sub-groups

	public virtual BlightTokenBindingNoEvents Blight => new BlightTokenBindingNoEvents( this );
	public IDefendTokenBinding Defend => new DefendTokenBinding( this );
	public TokenBindingNoEvents Beasts => new ( this, TokenType.Beast );
	public TokenBindingNoEvents Disease => new ( this, TokenType.Disease );
	public TokenBindingNoEvents Wilds => new ( this, TokenType.Wilds );
	public TokenBindingNoEvents Badlands => new ( this, TokenType.Badlands ); // This should not be used directly from inside Actions
	public DahanGroupBindingNoEvents Dahan => new DahanGroupBindingNoEvents( this );

	#endregion

	#region private

	static void ValidateNotDead( Token specific ) {
		if(specific is HealthToken ht && ht.RemainingHealth == 0) 
			throw new ArgumentException( "We don't store dead counts" );
	}

	public readonly CountDictionary<Token> _counts; // !!! public for Tokens_ForIsland Memento, create own momento.
	readonly IIslandTokenApi _api;
	readonly GameState _gameState; // !! merge this usage into token api, I guess.  Here for access to island.

	#endregion

	#region Non-event Generationg Token Changes

	/// <summary> Non-event-triggering setup </summary>
	public void Adjust( Token specific, int delta ) {
		if(specific is HealthToken ht && ht.RemainingHealth == 0) 
			throw new System.ArgumentException( "Don't try to track dead tokens." );
		_counts[specific] += delta;
	}

	public void InitDefault( HealthTokenClass tokenClass, int value )
		=> Init( GetDefault( tokenClass ), value );

	public Task AddDefault( HealthTokenClass tokenClass, int count, UnitOfWork actionScope, AddReason addReason = AddReason.Added )
		=> Add( GetDefault( tokenClass ), count, actionScope, addReason );

	public void AdjustDefault( HealthTokenClass tokenClass, int delta ) 
		=> Adjust( GetDefault( tokenClass ), delta );

	public void Init( Token specific, int value ) {
		_counts[specific] = value;
	}

	public async Task AdjustHealthOfAll( int delta, UnitOfWork actionScope, params HealthTokenClass[] tokenClasses ) {
		if(delta == 0) return;
		foreach(var tokenClass in tokenClasses) {
			var tokens = OfClass( tokenClass ).Cast<HealthToken>();
			var orderedTokens = delta < 0
				? tokens.OrderBy( x => x.FullHealth ).ToArray()
				: tokens.OrderByDescending( x => x.FullHealth ).ToArray();
			foreach(var token in orderedTokens)
				await AdjustHealthOf( token, delta, this[token], actionScope );
		}
	}

	public HealthToken GetDefault( HealthTokenClass tokenClass ) => this._api.GetDefault( tokenClass );

	public void ReplaceAllWith( Token original, Token replacement ) {
		Adjust( replacement, this[original] );
		Init( original, 0 );
	}

	public void ReplaceNWith( int countToReplace, Token oldToken, Token newToken ) {
		countToReplace = Math.Min( countToReplace, this[oldToken] );
		Adjust( oldToken, -countToReplace );
		Adjust( newToken, countToReplace );
	}

	/// <summary> Replaces (via adjust) HealthToken with new HealthTokens </summary>
	/// <returns> The # of remaining Adjusted tokens. </returns>
	public async Task<(HealthToken, int)> AdjustHealthOf( HealthToken token, int delta, int count, UnitOfWork actionScope ) {
		count = Math.Min( this[token], count );
		if(count == 0) return (token, 0);

		var newToken = token.AddHealth( delta ); // throws exception if health < 1

		if(newToken.IsDestroyed) {
			await this.Destroy( token, count, actionScope ); // destroy the old token
			return (token, 0);
		}

		Adjust( token, -count );
		Adjust( newToken, count );
		return (newToken, count);
	}

	#endregion


	#region Event-Generating Token Changes

	public async Task<TokenAddedArgs> Add( Token token, int count, UnitOfWork actionScope, AddReason addReason = AddReason.Added ) {
		TokenAddedArgs addResult = Add_Silent( token, count, actionScope, addReason );
		if(addResult != null) {
			addResult.GameState = this._api.AccessGameState();
			foreach(var handler in Keys.OfType<IHandleTokenAdded>().ToArray())
				await handler.HandleTokenAdded( addResult );
		}
		return addResult;
	}

	 TokenAddedArgs Add_Silent( Token token, int count, UnitOfWork actionScope, AddReason addReason = AddReason.Added ) {
		if(count < 0) throw new ArgumentOutOfRangeException( nameof( count ) );

		// Pre-Add check/adjust
		var addingArgs = new AddingTokenArgs( this, addReason, actionScope ) { Count = count, Token = token };
		foreach(var mod in Keys.OfType<IHandleAddingToken>().ToArray() )
			mod.ModifyAdding( addingArgs );

		if(addingArgs.Count < 0) throw new IndexOutOfRangeException( nameof( addingArgs.Count ) );
		if(addingArgs.Count == 0) return null;

		// Do Add
		this[addingArgs.Token] += addingArgs.Count;

		// Post-Add event
		return new TokenAddedArgs( this, addingArgs.Token, addReason, addingArgs.Count, actionScope );
	}


	/// <summary> returns null if no token removed </summary>
	public async Task<PublishTokenRemovedArgs> Remove( Token token, int count, UnitOfWork actionScope, RemoveReason reason = RemoveReason.Removed ) {
		var cmd = await Remove_Silent( token, count, actionScope, reason );
		if( cmd != null) {
			var e = cmd.MakeEvent(); // !!! clean this up.  Don't need cmd and event
			foreach(var handler in Keys.OfType<IHandleTokenRemoved>().ToArray() )
				await handler.HandleTokenRemoved( e );
		}
		return cmd;
	}

	/// <summary> returns null if no token removed. Does Not publish event.</summary>
	async Task<PublishTokenRemovedArgs> Remove_Silent( Token token, int count, UnitOfWork actionScope, RemoveReason reason = RemoveReason.Removed ) {
		count = System.Math.Min( count, this[token] );

		// Pre-Remove check/adjust
		var removingArgs = new RemovingTokenArgs( this, reason, actionScope ) { Count = count, Token = token };
		foreach(var mod in Keys.OfType<IHandleRemovingToken>().ToArray())
			await mod.ModifyRemoving( removingArgs );

		if(removingArgs.Count < 0) throw new IndexOutOfRangeException( nameof( removingArgs.Count ) );

		if(removingArgs.Count == 0) return null;

		// Do Remove
		this[removingArgs.Token] -= removingArgs.Count;

		// Post-Remove event
		return new PublishTokenRemovedArgs( removingArgs.Token, reason, actionScope, this, removingArgs.Count );
	}

	public GameState AccessGameState() => _api.AccessGameState();


	// Convenience only
	public Task Destroy( Token token, int count, UnitOfWork actionScope ) => token is HealthToken ht
		? ht.Destroy(this, count, actionScope)
		: Remove(token, count, actionScope, RemoveReason.Destroyed );

	/// <summary> Gathering / Pushing + a few others </summary>
	// !!! Powers should not use this Move directly, instead, they should go through TargetSpaceCtx so they can use custom Dahan and Invader bindings.
	public async Task MoveTo( Token token, Space destination, UnitOfWork actionScope ) {
		// Current implementation favors:
		//		switching token types prior to Add/Remove so events handlers don't switch token type
		//		perfoming the add/remove action After the Adding/Removing modifications

		// Possible problems with this method:
		//		The token in the Added event, may be different than token that was attempted to be added.
		//		The Token in the Removed event, may be a different token than was requested to be removed.
		//		The token Added may be Different than the token Removed
		//		If the Adding stops the and, what do we do about the token that was removed?
		//		Move requires a special Publish because it pertains to 2 spaces - we don't want to publish it twice (once for each space)

		// Possible solutions:
		//		Don't allow Adding to modify count
		//		Move has 2 tokens, token added and token removed

		if(this[token] == 0) return; // unable to remove desired token

		// Remove from source
		var removeResults = await Remove( token, 1, actionScope, RemoveReason.MovedFrom );
		if(removeResults is null) return; // can be prevented by Remove-Mods

		// Add to destination
		var dstTokens = _api.GetTokensFor( destination );
		var addResult = await dstTokens.Add( token, removeResults.Count, actionScope, AddReason.MovedTo );
		if( addResult == null) return;

		// Publish
		await _api.Publish_Moved( new TokenMovedArgs(this,dstTokens, actionScope ) {
			// removed
			TokenRemoved = removeResults.Token,
			// added
			TokenAdded = addResult.Token,
			// general
			Count = 1,
		} );

	}

	#endregion

	#region Invader Specific

	/// <summary> Includes dreaming invaders. </summary>
	public IEnumerable<HealthToken> InvaderTokens() => OfCategory( TokenCategory.Invader ).Cast<HealthToken>();

	public bool HasInvaders() => Has( TokenCategory.Invader );

	public bool HasStrife => Keys.OfType<HealthToken>().Any(x=>x.StrifeCount>0);

	public int CountStrife() => Keys.OfType<HealthToken>().Where(x=>x.StrifeCount>0).Sum( t => _counts[t] );

	public int TownsAndCitiesCount() => this.SumAny( Invader.Town, Invader.City );

	public int InvaderTotal() => InvaderTokens().Sum( i => _counts[i] );

	public async Task AddStrifeTo( HealthToken invader, UnitOfWork actionScope, int count = 1 ) {

		// Remove old type from 
		if(this[invader]<count)
			throw new ArgumentOutOfRangeException($"collection does not contain {count} {invader}");
		this[invader] -= count;

		// Add new strifed
		var strifed = invader.HavingStrife( invader.StrifeCount + 1 );
		this[strifed] += count;

		// !!! Adding / Removing a strife needs to trigger a token-change event for Observe the Ever Changing World
		// !!! Test that a ravage that does nothing but removes a strife, triggers Observe the Ever Changing World

		if( strifed.IsDestroyed ) // due to a strife-health penalty
			await Destroy( strifed, this[strifed], actionScope );
	}

	#endregion

	public HealthToken RemoveStrife( HealthToken orig, int tokenCount ) {
		HealthToken lessStrifed = orig.AddStrife( -1 );
		this[lessStrifed] += tokenCount;
		this[orig] -= tokenCount;
		return lessStrifed;
	}

	public int AttackDamageFrom1( HealthToken ht ) => ht.Class.Category == TokenCategory.Dahan ? 2
		: Math.Max( 0, _api.InvaderAttack( ht.Class ) - DamagePenaltyPerInvader );

	public int DamagePenaltyPerInvader = 0; // !!! ??? Does the Memento reset this back to 0?

	#region Adjacent Properties

	public IEnumerable<SpaceState> Adjacent { get {
		foreach(var space in Space.Adjacent)
			yield return _gameState.Tokens[space];

		if(LinkedViaWays != null && !LinkedViaWays.Space.InStasis)
			yield return LinkedViaWays;
	} }

	public SpaceState LinkedViaWays; // HACK - for Finder

	// This is trying to accomplish: (Some terrain other than Ocean)
	public IEnumerable<SpaceState> CascadingBlightOptions => Adjacent
		 .Where(x => !this._gameState.Island.Terrain_ForBlight.MatchesTerrain(x, Terrain.Ocean) // normal case,
			|| this._gameState.Island.Terrain_ForBlight.MatchesTerrain( x, Terrain.Wetland ) );

	public IEnumerable<SpaceState> Range(int maxDistance) => this.CalcDistances( maxDistance ).Keys;

	/// <summary> Explicitly named so not to confuse with Powers - Range commands. </summary>
	public IEnumerable<SpaceState> InOrAdjacentTo => Range( 1 );

	#endregion

	#region Skip API

	public void SkipAllInvaderActions( string label ) {
		Skip1Ravage( label );
		Skip1Build( label );
		Skip1Explore( label );
	}

	public void Skip1Ravage( string label ) => Adjust( new SkipRavage( label ), 1 );

	// !!! can we have 2 of these tokens?, will it throw an exception?, will it stop 2 builds?
	public void Skip1Build( string label ) => Adjust( SkipBuild.Default( label ), 1 );

	public void Skip1Explore( string label ) => Adjust( new SkipExploreTo( label ), 1 );

	public void SkipAllBuilds( string label, params TokenClass[] stoppedClasses ) {
		if(stoppedClasses == null || stoppedClasses.Length == 0)
			stoppedClasses = new TokenClass[] { Invader.Town, Invader.City };
		Adjust( new SkipBuild( label, UsageDuration.SkipAllThisTurn, stoppedClasses ), 1 );
	}

	#endregion


	public void TimePasses() {
		Blight.Blocked = false; // !!! move inside cleanup token???

		foreach( var cleanup in Keys.OfType<TokenWithEndOfRoundCleanup>().ToArray() )
			cleanup.EndOfRoundCleanup( this );
	}

}
