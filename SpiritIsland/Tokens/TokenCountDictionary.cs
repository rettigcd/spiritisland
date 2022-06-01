namespace SpiritIsland;

/// <summary>
/// Wraps: Space, Token-Counts on that space, API to publish token-changed events.
/// </summary>
public class TokenCountDictionary {

	#region constructor

	public TokenCountDictionary( Space space, CountDictionary<Token> counts, IIslandTokenApi tokenApi ) {
		this.Space = space;
		this.counts = counts;
		this.tokenApi = tokenApi;
	}

	/// <summary> Clone / copy constructor </summary>
	public TokenCountDictionary( TokenCountDictionary src ) {
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
				count += tokenApi.GetDynamicTokenFor(Space, ut);
			return count;
		}
		private set {
			ValidateNotDead( specific );
			counts[specific] = value; 
		}
	}

	public IEnumerable<Token> Keys => counts.Keys; // !! This won't list virtual (defend) tokens

	/// <summary> Ordered Alphabetically. </summary>
	public string Summary => counts.TokenSummary();

	public override string ToString() => Space.Label + ":" + Summary;

	public virtual BlightTokenBinding Blight => new BlightTokenBinding( this );
	public IDefendTokenBinding Defend => new DefendTokenBinding( this );
	public TokenBinding Beasts => new ( this, TokenType.Beast );
	public TokenBinding Disease => new ( this, TokenType.Disease );
	public TokenBinding Wilds => new ( this, TokenType.Wilds );
	public TokenBinding Badlands => new ( this, TokenType.Badlands ); // This should not be used directly from inside Actions
	public DahanGroupBindingNoEvents Dahan{
		get => _dahan ??= new DahanGroupBindingNoEvents( this ); // ! change the ??= to ?? and we would not need to hang on to the binding.
		set => _dahan = value; // Allows Dahan behavior to be overridden
	}
	DahanGroupBindingNoEvents _dahan;

	#region private

	static void ValidateNotDead( Token specific ) {
		if(specific is HealthToken ht && ht.RemainingHealth == 0) 
			throw new ArgumentException( "We don't store dead counts" );
	}

	public readonly CountDictionary<Token> counts; // !!! public for Tokens_ForIsland Memento, create own momento.
	readonly IIslandTokenApi tokenApi;

	#endregion

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

	public HealthToken GetDefault( HealthTokenClass tokenClass ) => this.tokenApi.GetDefault( tokenClass );

	public void Init( Token specific, int value ) {
		counts[specific] = value;
	}

	public Task Add( Token token, int count, Guid actionId, AddReason addReason = AddReason.Added ) {
		if(count < 0) throw new System.ArgumentOutOfRangeException( nameof( count ) );
		this[token] += count;
		return tokenApi.Publish_Added( Space, token, count, addReason, actionId );
	}

	/// <summary> returns null if no token removed </summary>
	public async Task<TokenRemovedArgs> Remove( Token token, int count, Guid actionId, RemoveReason reason = RemoveReason.Removed ) {
		count = System.Math.Min( count, this[token] );
		var args = new RemovingTokenArgs { ActionId = actionId, Count = count, Space = Space, Reason = reason, Token = token };
		await tokenApi.Publish_Removing( args );
		if(args.Count == 0) return null;
		if(args.Count < 0) throw new System.ArgumentOutOfRangeException( nameof( args.Count ) );
		this[args.Token] -= args.Count;
		var removedArgs = new TokenRemovedArgs( token, reason, actionId, Space, count );
		await tokenApi.Publish_Removed( removedArgs );
		return removedArgs;
	}

	// Convenience only
	public Task Destroy( Token token, int count, Guid actionId ) => Remove(token, count, actionId, RemoveReason.Destroyed );

	public async Task MoveTo(Token token, Space destination, Guid actionId ) {

		// Remove from source
		if( token.Class != TokenType.Dahan)
			Adjust( token, -1 );
		else if( ! (await Dahan.Bind(actionId).Remove1(token, RemoveReason.MovedFrom)) ) // !!! Moving publishes a Move event, don't publish this Remove event
			return;

		// Add to destination
		tokenApi.GetTokensFor( destination ).Adjust( token, 1 );

		// Publish
		await tokenApi.Publish_Moved( token, Space, destination, actionId );

	}

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

	// !! This is starting to be a Space-State, not just a token Dictionary, starting to store more here than just tokens
	public int AttackDamageFrom1( HealthToken ht ) => ht.Class.Category == TokenCategory.Dahan ? ht.Class.Attack
		: Math.Max( 0, ht.Class.Attack - DamagePenaltyPerInvader );

	public int DamagePenaltyPerInvader = 0; // !!! ??? Does the Memento reset this back to 0?

}