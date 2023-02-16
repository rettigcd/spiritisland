namespace SpiritIsland;

/// <summary>
/// Wraps: Space, Token-Counts on that space, API to publish token-changed events.
/// Has same Scope as GameState (not bound to an ActionScope
/// </summary>
public class SpaceState : ISeeAllNeighbors<SpaceState> {

	#region constructor

	public SpaceState( Space space, CountDictionary<ISpaceEntity> counts, IIslandTokenApi tokenApi ) {
		Space = space;
		_counts = counts;
		_api = tokenApi;
	}

	/// <summary> Clone / copy constructor </summary>
	protected SpaceState( SpaceState src ) {
		Space = src.Space;
		_counts = src._counts;
		_api = src._api;
	}

	#endregion

	public Space Space { get; }

	public int this[ISpaceEntity specific] {
		get {
			ValidateNotDead( specific );
			int count = _counts[specific];
			if( specific is TokenClassToken ut )
				count += _api.GetDynamicTokensFor(this, ut);
			return count;
		}
	}

	public IEnumerable<ISpaceEntity> Keys => _counts.Keys; // !! This won't list virtual (defend) tokens

	#region Enumeration / Detection(HaS) / Summing

	public ISpaceEntity[] OfCategory( TokenCategory category ) => OfCategoryInternal( category ).ToArray();
	public ISpaceEntity[] OfClass( IEntityClass tokenClass ) => OfClassInternal( tokenClass ).ToArray();
	public ISpaceEntity[] OfAnyClass( params IEntityClass[] classes ) => OfAnyClassInternal( classes ).ToArray(); // !! This could *probably* return IVisibleToken
	public IToken[] RemovableOfAnyClass( RemoveReason reason, params IEntityClass[] classes ) {
		var stoppers = Keys.OfType<IHandleRemovingToken>();
		return OfAnyClass( classes ).Cast<IToken>()
			.Where( token => {
				var removingArgs = new RemovingTokenArgs( this, reason, RemoveMode.Test ) { Count = 1, Token = token };
				foreach(var stopper in stoppers)
					stopper.ModifyRemoving(removingArgs);
				return removingArgs.Count == 1;
			} )
			.ToArray();
	}

	public HumanToken[] OfHumanClass( HumanTokenClass tokenClass ) => OfClassInternal( tokenClass ).Cast<HumanToken>().ToArray();
	public HumanToken[] OfAnyHumanClass( params HumanTokenClass[] classes ) => OfAnyClassInternal( classes ).Cast<HumanToken>().ToArray();

	// 2 SpaceToken helper classes
	public IEnumerable<SpaceToken> SpaceTokensOfClass( IEntityClass tokenClass ) 
		=> OfClassInternal( tokenClass ).OfType<IToken>().Select(x=>new SpaceToken(Space,x));
	public IEnumerable<SpaceToken> SpaceTokensOfAnyClass( params IEntityClass[] tokenClasses )
		=> OfAnyClassInternal( tokenClasses ).OfType<IToken>().Select( x => new SpaceToken( Space, x ) );

	protected IEnumerable<ISpaceEntity> OfCategoryInternal( TokenCategory category ) => Keys.Where( k => k.Class.Category == category );
	protected IEnumerable<ISpaceEntity> OfClassInternal( IEntityClass tokenClass ) => Keys.Where( x => x.Class == tokenClass );
	protected IEnumerable<ISpaceEntity> OfAnyClassInternal( params IEntityClass[] classes ) => Keys.Where( specific => classes.Contains( specific.Class ) );

	public bool Has( IEntityClass inv ) => OfClassInternal( inv ).Any();
	public bool Has( TokenCategory cat ) => OfCategoryInternal( cat ).Any();
	public bool HasAny( params IEntityClass[] healthyInvaders ) => OfAnyClassInternal( healthyInvaders ).Any();

	public int Sum( IEntityClass tokenClass ) => OfClassInternal( tokenClass ).Sum( k => _counts[k] );
	public int Sum( TokenCategory category ) => OfCategoryInternal( category ).Sum( k => _counts[k] );
	public int SumAny( params IEntityClass[] healthyInvaders ) => OfAnyClassInternal( healthyInvaders ).Sum( k => _counts[k] );

	#endregion

	#region To-String methods

	/// <summary>Gets all tokens that have a SpaceAbreviation</summary>
	public string Summary => _counts.TokenSummary();

	public override string ToString() => Space.Label + ":" + Summary;

	#endregion

	#region Token-Type Sub-groups

	public virtual BlightTokenBinding Blight => new BlightTokenBinding( this );
	public IDefendTokenBinding Defend => new DefendTokenBinding( this );
	public BeastBinding Beasts => new ( this, Token.Beast );
	public TokenBinding Disease => new ( this, _api.GetDefault( Token.Disease ) );
	public TokenBinding Wilds => new ( this, Token.Wilds );
	public virtual TokenBinding Badlands => new ( this, Token.Badlands ); // This should not be used directly from inside Actions
	public HealthTokenClassBinding Dahan => new HealthTokenClassBinding( this, Human.Dahan );
	//public HealthTokenClassBinding_NoEvents Explorers => new HealthTokenClassBinding_NoEvents( this, Invader.Explorer );
	//public HealthTokenClassBinding_NoEvents Towns => new HealthTokenClassBinding_NoEvents( this, Invader.Town );
	//public HealthTokenClassBinding_NoEvents Cities => new HealthTokenClassBinding_NoEvents( this, Invader.City );

	#endregion

	#region fields

	static void ValidateNotDead( ISpaceEntity specific ) {
		if(specific is HumanToken ht && ht.RemainingHealth == 0) 
			throw new ArgumentException( "We don't store dead counts" );
	}

	readonly CountDictionary<ISpaceEntity> _counts;
	protected readonly IIslandTokenApi _api;

	#endregion

	#region Non-event Generationg Token Changes

	/// <summary> Non-event-triggering setup </summary>
	public void Adjust( ISpaceEntity specific, int delta ) {
		if(specific is HumanToken human && human.RemainingHealth == 0) 
			throw new System.ArgumentException( "Don't try to track dead tokens." );
		if(specific is ITrackMySpaces selfTracker) AdjustTrackedToken( selfTracker, delta);
		_counts[specific] += delta;
	}

	public void Init( ISpaceEntity specific, int newValue ) {
		int old = _counts[specific];
		Adjust( specific, newValue-old ); // go through Adjust so that we keep ITrackMySpaces in sync
	}

	public void InitDefault( HumanTokenClass tokenClass, int value )
		=> Init( GetDefault( tokenClass ), value );

	public void AdjustDefault( HumanTokenClass tokenClass, int delta ) 
		=> Adjust( GetDefault( tokenClass ), delta );

	public HumanToken GetDefaultHuman( HumanTokenClass tokenClass ) => (HumanToken)_api.GetDefault( tokenClass );
	public IToken GetDefault( IEntityClass tokenClass ) => _api.GetDefault( tokenClass );

	public void ReplaceAllWith( ISpaceEntity original, ISpaceEntity replacement ) {
		Adjust( replacement, this[original] );
		Init( original, 0 );
	}
	public void ReplaceNWith( int countToReplace, ISpaceEntity oldToken, ISpaceEntity newToken ) {
		countToReplace = Math.Min( countToReplace, this[oldToken] );
		Adjust( oldToken, -countToReplace );
		Adjust( newToken, countToReplace );
	}

	#endregion

	#region Invader Specific

	/// <summary> Includes dreaming invaders. </summary>
	public IEnumerable<HumanToken> InvaderTokens() => OfCategory( TokenCategory.Invader ).Cast<HumanToken>();

	public bool HasInvaders() => Has( TokenCategory.Invader );

	public bool HasStrife => Keys.OfType<HumanToken>().Any(x=>0<x.StrifeCount);
	public int StrifeCount => Keys.OfType<HumanToken>().Sum( x => x.StrifeCount );

	public int CountStrife() => Keys.OfType<HumanToken>().Where(x=>x.StrifeCount>0).Sum( t => _counts[t] );

	public int TownsAndCitiesCount() => this.SumAny( Human.Town_City );

	public int InvaderTotal() => InvaderTokens().Sum( i => _counts[i] );

	#endregion

	public HumanToken RemoveStrife( HumanToken orig, int tokenCount ) {
		HumanToken lessStrifed = orig.AddStrife( -1 );
		Adjust(lessStrifed, tokenCount);
		Adjust(orig, -tokenCount);
		return lessStrifed;
	}

	public int AttackDamageFrom1( HumanToken ht ) => ht.Class.Category == TokenCategory.Dahan ? 2
		: _api.InvaderAttack( ht.Class );

	#region Adjacent Properties

	public IEnumerable<SpaceState> Adjacent_Existing { 
		get {
			foreach(var space in Space.Adjacent_Existing)
				yield return space.Tokens;

			foreach(var gateway in Keys.OfType<GatewayToken>())
				yield return gateway.GetLinked(this);
		}
	}

	public IEnumerable<SpaceState> Adjacent => Adjacent_Existing.IsInPlay();

	public IEnumerable<SpaceState> Adjacent_ForInvaders => IsConnected ? Adjacent.Where( x => x.IsConnected ) : Enumerable.Empty<SpaceState>();

	public IEnumerable<SpaceState> Range(int maxDistance) => this.CalcDistances( maxDistance ).Keys.IsInPlay();

	/// <summary> Explicitly named so not to confuse with Powers - Range commands. </summary>
	public IEnumerable<SpaceState> InOrAdjacentTo => Range( 1 );

	/// <summary> Has no Isolate token. </summary>
	public bool IsConnected => this[Token.Isolate] == 0;

	#endregion

	#region Skip API

	public void SkipAllInvaderActions( string label ) {
		SkipRavage( label, UsageDuration.SkipAllThisTurn );
		SkipAllBuilds( label );
		Adjust( new SkipExploreTo(skipAll:true), 1 );
	}

	public void SkipRavage( string label, UsageDuration duration = UsageDuration.SkipOneThisTurn ) => Adjust( new SkipRavage(label, duration), 1 );

	public void Skip1Build( string label ) => Adjust( SkipBuild.Default( label ), 1 );

	public void SkipAllBuilds( string label, params IEntityClass[] stoppedClasses ) => Adjust( new SkipBuild( label, UsageDuration.SkipAllThisTurn, stoppedClasses ), 1 );

	public void Skip1Explore( string _ ) => Adjust( new SkipExploreTo(), 1 );

	#endregion

	#region Ravage

	/// <summary> Does 1 potential Ravage (if no stopper tokens) </summary>
	public Task Ravage() {
		GameState gameState = GameState.Current;
		RavageBehavior cfg = gameState.GetRavageConfiguration( Space );
		return cfg.Exec( this, gameState );
	}

	#endregion

	#region Ocean Helpers

	void AdjustTrackedToken( ITrackMySpaces token, int delta ) {
		_api.Adjust(token,Space,delta);
	}

	#endregion

	public void TimePasses() {
		foreach(var cleanup in Keys.OfType<ISpaceEntityWithEndOfRoundCleanup>().ToArray())
			cleanup.EndOfRoundCleanup( this );
	}

	#region GetHashCode and Equals

	// Utter a Curse of Dread and Bone & Bargains of Power require these overrides:
	public override int GetHashCode() => Space.GetHashCode();
	public override bool Equals( object obj ) => obj is SpaceState other && other.Space == Space;
	public static bool operator ==( SpaceState ss1, SpaceState ss2 ) => Object.ReferenceEquals( ss1, ss2) || ss1.Equals(ss2);
	public static bool operator !=( SpaceState ss1, SpaceState ss2 ) => !(ss1==ss2);

	#endregion

	// It is questionable if this should be here since adjusting shouldn't make any difference
	// but in this case, it COULD destroy a token.

	public async Task AdjustHealthOfAll( int delta, params HumanTokenClass[] tokenClasses ) {
		if(delta == 0) return;
		foreach(var tokenClass in tokenClasses) {
			var tokens = OfHumanClass( tokenClass );
			var orderedTokens = delta < 0
				? tokens.OrderBy( x => x.FullHealth ).ToArray()
				: tokens.OrderByDescending( x => x.FullHealth ).ToArray();
			foreach(var token in orderedTokens)
				await AdjustHealthOf( token, delta, this[token] );
		}
	}

	public async Task AddRemoveStrifeTo( HumanToken invader, int count = 1 ) {

		// Remove old type from 
		if(this[invader] < count)
			throw new ArgumentOutOfRangeException( $"collection does not contain {count} {invader}" );
		Adjust( invader, -count );

		// Add new strifed
		var strifed = invader.HavingStrife( invader.StrifeCount + 1 );
		Adjust( strifed, count );

		// !!! Adding / Removing a strife needs to trigger a token-change event for Observe the Ever Changing World
		// !!! Test that a ravage that does nothing but removes a strife, triggers Observe the Ever Changing World

		if(strifed.IsDestroyed) // due to a strife-health penalty
			await Destroy( strifed, this[strifed] );
	}

	/// <summary> Replaces (via adjust) HealthToken with new HealthTokens </summary>
	/// <returns> The # of remaining Adjusted tokens. </returns>
	public async Task<(HumanToken, int)> AdjustHealthOf( HumanToken token, int delta, int count ) {
		count = Math.Min( this[token], count );
		if(count == 0) return (token, 0);

		var newToken = token.AddHealth( delta ); // throws exception if health < 1

		if(newToken.IsDestroyed) {
			await Destroy( token, count ); // destroy the old token
			GameState.Current.LogDebug($"{Space.Text} Adjusting {count} {token.SpaceAbreviation} to {newToken.SpaceAbreviation} => Destroyed!");
			return (token, 0);
		}

		Adjust( token, -count );
		Adjust( newToken, count );
		GameState.Current.LogDebug( $"Adjusting {count} {token.SpaceAbreviation} to {newToken.SpaceAbreviation}" );
		return (newToken, count);
	}

	public Task AddDefault( IEntityClass tokenClass, int count, AddReason addReason = AddReason.Added )
		=> Add( GetDefault( tokenClass ), count, addReason );


	// Convenience only
	public Task Destroy( IToken token, int count ) => token is HumanToken ht
		? ht.Destroy( this, count )
		: Remove( token, count, RemoveReason.Destroyed );

	public async Task<TokenAddedArgs> Add( IToken token, int count, AddReason addReason = AddReason.Added ) {
		TokenAddedArgs addResult = Add_Silent( token, count, addReason );
		if(addResult != null)
			foreach(IHandleTokenAdded handler in Keys.OfType<IHandleTokenAdded>().ToArray())
				await handler.HandleTokenAdded( addResult );
		return addResult;
	}

	TokenAddedArgs Add_Silent( IToken token, int count, AddReason addReason = AddReason.Added ) {
		if(count < 0) throw new ArgumentOutOfRangeException( nameof( count ) );

		// Pre-Add check/adjust
		var addingArgs = new AddingTokenArgs( this, addReason ) { Count = count, Token = token };
		foreach(var mod in Keys.OfType<IHandleAddingToken>().ToArray())
			mod.ModifyAdding( addingArgs );

		if(addingArgs.Count < 0) throw new IndexOutOfRangeException( nameof( addingArgs.Count ) );
		if(addingArgs.Count == 0) return null;

		// Do Add
		Adjust( addingArgs.Token, addingArgs.Count );

		// Post-Add event
		return new TokenAddedArgs( this, addingArgs.Token, addReason, addingArgs.Count );
	}


	/// <summary> returns null if no token removed </summary>
	public virtual async Task<TokenRemovedArgs> Remove( IToken token, int count, RemoveReason reason = RemoveReason.Removed ) {
		if( reason == RemoveReason.MovedFrom )
			throw new ArgumentException("Moving Tokens must be done from the .Move method for events to work properly",nameof(reason));

		// grab event handlers BEFORE the token is removed, so token can self-handle its own removal
		var tokenRemovedHandlers = GrabHandlers_TokenRemoved();

		var e = await Remove_Silent( token, count, reason );
		if(e == null) return null;

		foreach(IHandleTokenRemoved handler in tokenRemovedHandlers)
			await handler.HandleTokenRemoved( e );

		return e;
	}

	// grab event handlers BEFORE the token is removed, so token can self-handle its own removal
	IHandleTokenRemoved[] GrabHandlers_TokenRemoved() => Keys.OfType<IHandleTokenRemoved>().ToArray();

	/// <summary> returns null if no token removed. Does Not publish event.</summary>
	protected virtual async Task<TokenRemovedArgs> Remove_Silent( IToken token, int count, RemoveReason reason = RemoveReason.Removed ) {
		count = System.Math.Min( count, this[token] );

		// Pre-Remove check/adjust
		var removingArgs = new RemovingTokenArgs( this, reason, RemoveMode.Live ) { Count = count, Token = token };
		foreach(var mod in Keys.OfType<IHandleRemovingToken>().ToArray())
			await mod.ModifyRemoving( removingArgs );

		if(removingArgs.Count < 0) throw new IndexOutOfRangeException( nameof( removingArgs.Count ) );

		if(removingArgs.Count == 0) return null;

		// Do Remove
		Adjust( removingArgs.Token, -removingArgs.Count );

		// Post-Remove event
		return new TokenRemovedArgs( removingArgs.Token, reason, this, removingArgs.Count );

	}

	/// <summary> Gathering / Pushing + a few others </summary>
	public async Task<TokenMovedArgs> MoveTo( IToken token, SpaceState dstTokens ) {
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

		if(this[token] == 0) return null; // unable to remove desired token
		var tokenRemovedHandlers = GrabHandlers_TokenRemoved();

		// Remove from source
		TokenRemovedArgs removeResult = await Remove_Silent( token, 1, RemoveReason.MovedFrom );
		if(removeResult == null) return null;

		// Add to destination
		TokenAddedArgs addResult = dstTokens.Add_Silent( /* Modified, NOT original */ removeResult.Removed, 1, AddReason.MovedTo );
		if(addResult == null) return null;

		// Publish
		var tokenMoved = new TokenMovedArgs( removeResult, addResult );

		foreach(IHandleTokenRemoved handler in tokenRemovedHandlers)
			await handler.HandleTokenRemoved( tokenMoved );

		foreach(IHandleTokenAdded handler in dstTokens.Keys.OfType<IHandleTokenAdded>().ToArray())
			await handler.HandleTokenAdded( tokenMoved );

		return tokenMoved;
	}

	public virtual HumanToken GetNewDamagedToken( HumanToken invaderToken, int availableDamage )
		=> invaderToken.AddDamage( availableDamage );

	public virtual Task<int> DestroyNInvaders( HumanToken invaderToDestroy, int countToDestroy ) {
		return invaderToDestroy.Destroy( this, countToDestroy );
	}

	public virtual TokenGatherer Gather( Spirit self ) => new TokenGatherer( self, this );

	public virtual TokenPusher Pusher( Spirit self ) => new TokenPusher( self, this );

	#region Bonus Damage

	// pass in null if we don't need to track original damage (like 1 damage per)
	// pass in a # if this is joining with original damage and needs tracked.
	public BonusDamage BonusDamageForAction( int? trackOriginalDamage = null ) => new BonusDamage( DamagePool.BadlandDamage( this, "Invaders" ), DamagePool.BonusDamage(), trackOriginalDamage );
	public BonusDamage BadlandDamageForDahan( int? trackOriginalDamage = null ) => new BonusDamage( DamagePool.BadlandDamage( this, "Dahan" ), new DamagePool(0), trackOriginalDamage );

	#endregion

}

/// <summary>
/// Tracks Bonus Damage for the Action based on # of Badlands and Spirit-bonus.
/// </summary>
public class BonusDamage {
	readonly int _originalDamage;
	readonly DamagePool _badlands;
	readonly DamagePool _bonus;

	/// <summary> May or may not have Original damage.  Track it and acount for it. </summary>
	public BonusDamage( DamagePool badlands, DamagePool bonusDamage, int? originalDamage ) {
		_badlands = badlands;
		_bonus = bonusDamage;

		if(originalDamage.HasValue) {
			// only triggers if there was actual damage done and we need to account for it.
			_originalDamage = originalDamage.Value;

			Available = originalDamage.Value + _bonus.Remaining;
			if(0 < originalDamage)
				Available += _badlands.Remaining;
		} else {
			// Original damage is known to have happened and we don't need to track it.
			_originalDamage = 0;

			Available = _bonus.Remaining + _badlands.Remaining;
		}
	}

	public int Available { get; }

	public void TrackDamageDone( int damageApplied ) {
		// Remove bonus damage from damage pools
		int poolDamageToAccountFor = damageApplied - _originalDamage;
		poolDamageToAccountFor -= _badlands.ReducePoolDamage( poolDamageToAccountFor );
		poolDamageToAccountFor -= _bonus.ReducePoolDamage( poolDamageToAccountFor );

		if(poolDamageToAccountFor > 0)
			throw new Exception( "somehow we did more damage than we have available" );
	}
}

public class DamagePool {

	public static DamagePool BadlandDamage( SpaceState ss, string groupName ) {
		// Note - this locks in Badland Count the 1st time we do damage.  Adding badlands after that has no effect.
		var actionScope = ActionScope.Current;
		string key = "BadlandDamage_" + ss.Space.Label +"_" + groupName;
		if(actionScope.ContainsKey( key )) return (DamagePool)actionScope[key];
		var pool = new DamagePool( ss.Badlands.Count );
		actionScope[key] = pool;
		return pool;
	}

	public static DamagePool BonusDamage() {
		// Note - this locks in Badland Count the 1st time we do damage.  Adding badlands after that has no effect.
		var actionScope = ActionScope.Current;
		string key = "BonusDamage";
		if(actionScope.ContainsKey( key )) return (DamagePool)actionScope[key];
		var pool = new DamagePool( actionScope?.Owner?.BonusDamage ?? 0 );
		actionScope[key] = pool;
		return pool;
	}

	public DamagePool( int init ) { remaining = init; }

	public int ReducePoolDamage( int poolDamageToAccountFor ) {
		int damageFromBadlandPool = Math.Min( remaining, poolDamageToAccountFor );
		remaining -= damageFromBadlandPool;
		return damageFromBadlandPool;
	}

	int remaining;
	public int Remaining => remaining;
}
