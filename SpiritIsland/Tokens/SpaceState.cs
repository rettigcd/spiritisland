namespace SpiritIsland;

/// <summary>
/// Wraps: Space, Token-Counts on that space, API to publish token-changed events.
/// </summary>
public class SpaceState : HasNeighbors<SpaceState> {

	#region constructor

	public SpaceState( Space space, CountDictionary<Token> counts, IIslandTokenApi tokenApi ) {
		this.Space = space;
		this.counts = counts;
		this.tokenApi = tokenApi;
	}

	/// <summary> Clone / copy constructor </summary>
	public SpaceState( SpaceState src ) {
		this.Space = src.Space;
		counts = src.counts.Clone();
		tokenApi = src.tokenApi;
	}

	#endregion

	public Space Space { get; }

	public int this[Token specific] {
		get {
			ValidateNotDead( specific );
			int count = counts[specific];
			if( specific is UniqueToken ut )
				count += tokenApi.GetDynamicTokensFor(this, ut);
			return count;
		}
		private set {
			ValidateNotDead( specific );
			counts[specific] = value; 
		}
	}

	public IEnumerable<Token> Keys => counts.Keys; // !! This won't list virtual (defend) tokens

	#region Generic - Single

	public Token[] OfType( TokenClass tokenClass )
		=> Keys.Where( x => x.Class == tokenClass ).ToArray();

	public bool Has( TokenClass inv )
		=> OfType( inv ).Any();

	public int Sum( TokenClass tokenClass )
		=> OfType( tokenClass ).Sum( k => counts[k] );

	#endregion

	#region Generic - Multiple (Any)

	public Token[] OfAnyType( params TokenClass[] healthyTypes )
		=> Keys.Where( specific => healthyTypes.Contains( specific.Class ) ).ToArray();

	public HealthToken[] OfAnyType( params HealthTokenClass[] healthyTypes )
		=> Keys.Where( specific => healthyTypes.Contains( specific.Class ) ).Cast<HealthToken>().ToArray();

	public bool HasAny( params TokenClass[] healthyInvaders )
		=> OfAnyType( healthyInvaders ).Any();

	public int SumAny( params TokenClass[] healthyInvaders )
		=> OfAnyType( healthyInvaders ).Sum( k => counts[k] );


	#endregion

	#region To-String methods

	public string Summary => counts.TokenSummary();

	public override string ToString() => Space.Label + ":" + Summary;

	#endregion

	#region Token-Type Sub-groups

	public virtual BlightTokenBindingNoEvents Blight => new BlightTokenBindingNoEvents( this );
	public IDefendTokenBinding Defend => new DefendTokenBinding( this );
	public TokenBindingNoEvents Beasts => new ( this, TokenType.Beast );
	public TokenBindingNoEvents Disease => new ( this, TokenType.Disease );
	public TokenBindingNoEvents Wilds => new ( this, TokenType.Wilds );
	public TokenBindingNoEvents Badlands => new ( this, TokenType.Badlands ); // This should not be used directly from inside Actions
	public DahanGroupBindingNoEvents Dahan{
		get => _dahan ??= new DahanGroupBindingNoEvents( this ); // ! change the ??= to ?? and we would not need to hang on to the binding.
		set => _dahan = value; // Allows Dahan behavior to be overridden
	}
	DahanGroupBindingNoEvents _dahan;

	#endregion

	#region private

	static void ValidateNotDead( Token specific ) {
		if(specific is HealthToken ht && ht.RemainingHealth == 0) 
			throw new ArgumentException( "We don't store dead counts" );
	}

	public readonly CountDictionary<Token> counts; // !!! public for Tokens_ForIsland Memento, create own momento.
	readonly IIslandTokenApi tokenApi;

	#endregion

	#region Non-event Generationg Token Changes

	/// <summary> Non-event-triggering setup </summary>
	public void Adjust( Token specific, int delta ) {
		if(specific is HealthToken ht && ht.RemainingHealth == 0) 
			throw new System.ArgumentException( "Don't try to track dead tokens." );
		counts[specific] += delta;
	}

	public void InitDefault( HealthTokenClass tokenClass, int value )
		=> Init( GetDefault( tokenClass ), value );

	public Task AddDefault( HealthTokenClass tokenClass, int count, Guid actionId, AddReason addReason = AddReason.Added )
		=> Add( GetDefault( tokenClass ), count, actionId, addReason );

	public void AdjustDefault( HealthTokenClass tokenClass, int delta ) 
		=> Adjust( GetDefault( tokenClass ), delta );

	public void Init( Token specific, int value ) {
		counts[specific] = value;
	}

	public async Task AdjustHealthOfAll( int delta, Guid actionId, params HealthTokenClass[] tokenClasses ) {
		if(delta == 0) return;
		foreach(var tokenClass in tokenClasses) {
			var tokens = OfType( tokenClass ).Cast<HealthToken>();
			var orderedTokens = delta < 0
				? tokens.OrderBy( x => x.FullHealth ).ToArray()
				: tokens.OrderByDescending( x => x.FullHealth ).ToArray();
			foreach(var token in orderedTokens)
				await AdjustHealthOf( token, delta, this[token], actionId );
		}
	}

	public HealthToken GetDefault( HealthTokenClass tokenClass ) => this.tokenApi.GetDefault( tokenClass );

	#endregion

	/// <summary> Replaces (via adjust) HealthToken with new HealthTokens </summary>
	/// <returns> The # of remaining Adjusted tokens. </returns>
	public async Task<(HealthToken,int)> AdjustHealthOf( HealthToken token, int delta, int count, Guid actionId ) {
		count = Math.Min( this[token], count );
		if(count == 0) return (token,0);

		var newToken = token.AddHealth( delta ); // throws exception if health < 1

		if(newToken.IsDestroyed) {
			await this.Destroy(token, count, actionId ); // destroy the old token
			return (token,0);
		}

		Adjust( token, -count );
		Adjust( newToken, count );
		return (newToken,count);
	}

	#region Event-Generating Token Changes

	public async Task Add( Token token, int count, Guid actionId, AddReason addReason = AddReason.Added ) {
		if(count < 0) throw new System.ArgumentOutOfRangeException( nameof( count ) );

		// Pre-Add check/adjust
		var addingArgs = new AddingTokenArgs { ActionId = actionId, Count = count, Space = Space, Reason = addReason, Token = token };
		await tokenApi.Publish_Adding( addingArgs );
		if(addingArgs.Count < 0) throw new IndexOutOfRangeException( nameof( addingArgs.Count ) );
		if(addingArgs.Count == 0) return;

		// Do Add
		this[addingArgs.Token] += addingArgs.Count;

		// Post-Add event
		await tokenApi.Publish_Added( new TokenAddedArgs( this, addingArgs.Token, addReason, addingArgs.Count, actionId ) );
	}

	/// <summary> returns null if no token removed </summary>
	public async Task<TokenRemovedArgs> Remove( Token token, int count, Guid actionId, RemoveReason reason = RemoveReason.Removed ) {
		count = System.Math.Min( count, this[token] );

		// Pre-Remove check/adjust
		var removingArgs = new RemovingTokenArgs { ActionId = actionId, Count = count, Space = Space, Reason = reason, Token = token };
		await tokenApi.Publish_Removing( removingArgs );
		if(removingArgs.Count < 0) throw new IndexOutOfRangeException( nameof( removingArgs.Count ) );
		if(removingArgs.Count == 0) return null;

		// Do Remove
		this[removingArgs.Token] -= removingArgs.Count;

		// Post-Remove event
		var removedArgs = new TokenRemovedArgs( removingArgs.Token, reason, actionId, Space, removingArgs.Count );
		await tokenApi.Publish_Removed( removedArgs );

		return removedArgs;
	}

	// Convenience only
	public Task Destroy( Token token, int count, Guid actionId ) => Remove(token, count, actionId, RemoveReason.Destroyed );

	/// <summary> Gathering / Pushing + a few others </summary>
	public async Task MoveTo(Token token, Space destination, Guid actionId ) {

		// Remove from source
		TokenRemovedArgs removedArgs;
		if( token.Class == TokenType.Dahan) {
			Token removedToken = await Dahan.Bind( actionId ).Remove1( RemoveReason.MovedFrom, token );
			if(removedToken == null) return;
			removedArgs = new TokenRemovedArgs( removedToken, RemoveReason.MovedFrom, actionId, Space, 1); // !!!
		} else
			removedArgs = await Remove( token,1,actionId, RemoveReason.MovedFrom );

		// Add to destination
		await tokenApi.GetTokensFor( destination )
			.Add( removedArgs.Token, removedArgs.Count, actionId, AddReason.MovedTo );

		// Publish
		await tokenApi.Publish_Moved( new TokenMovedArgs {
			Token = token,
			Class = token.Class,
			RemovedFrom = Space,
			AddedTo = destination,
			Count = 1,
			ActionId = actionId
		} );

	}

	#endregion

	#region Invader Specific

	public IEnumerable<HealthToken> InvaderTokens() => this.OfAnyType( Invader.City, Invader.Town, Invader.Explorer );

	public bool HasInvaders() => InvaderTokens().Any();

	public bool HasStrife => Keys.OfType<HealthToken>().Any(x=>x.StrifeCount>0);

	public int CountStrife() => Keys.OfType<HealthToken>().Where(x=>x.StrifeCount>0).Sum( t => counts[t] );

	public int TownsAndCitiesCount() => this.SumAny( Invader.Town, Invader.City );

	public int InvaderTotal() => InvaderTokens().Sum( i => counts[i] );

	public async Task AddStrifeTo( HealthToken invader, Guid actionId, int count = 1 ) {

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
			await Destroy( strifed, this[strifed], actionId );
	}

	#endregion

	public HealthToken RemoveStrife( HealthToken orig, int tokenCount ) {
		HealthToken lessStrifed = orig.AddStrife( -1 );
		this[lessStrifed] += tokenCount;
		this[orig] -= tokenCount;
		return lessStrifed;
	}

	public int AttackDamageFrom1( HealthToken ht ) => ht.Class.Category == TokenCategory.Dahan 
		? ht.Class.Attack
		: Math.Max( 0, ht.Class.Attack - DamagePenaltyPerInvader );

	public int DamagePenaltyPerInvader = 0; // !!! ??? Does the Memento reset this back to 0?

	public bool InStasis {
		get => counts[TokenType.Stasis] > 0;
		set => counts [TokenType.Stasis] = value ? 1 : 0;
	}

	public IEnumerable<SpaceState> Adjacent => PowerUp( Space.Adjacent );

	public IEnumerable<SpaceState> Range(int maxDistance) => this.CalcDistances( maxDistance ).Keys;
		
	IEnumerable<SpaceState> PowerUp(IEnumerable<Space> spaces) => this.tokenApi.PowerUp( spaces );
}