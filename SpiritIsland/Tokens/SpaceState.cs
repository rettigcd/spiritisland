namespace SpiritIsland;

/// <summary>
/// Wraps: Space, Token-Counts on that space, API to publish token-changed events.
/// Has same Scope as GameState (not bound to an ActionScope
/// </summary>
public class SpaceState 
	: ISeeAllNeighbors<SpaceState>
	, ILocation // !!! we don't SpaceToken or TokenOn using this as a location, maybe we should remove this
{

	#region constructor

	public SpaceState( Space space, CountDictionary<ISpaceEntity> counts, IEnumerable<ISpaceEntity> islandMods, IIslandTokenApi tokenApi ) {
		Space = space;
		_counts = counts;
		_islandMods = islandMods;
		_api = tokenApi;
	}

	/// <summary> Clone / copy constructor </summary>
	protected SpaceState( SpaceState src ) {
		Space = src.Space;
		_counts = src._counts;
		_islandMods = src._islandMods;
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

	// Misc
	public ITokenClass[] ClassesPresent => OfType<IToken>().Select( x => x.Class ).Distinct().ToArray();

	#region Enumeration / Detection(HaS) / Summing
	// ==================

	// -- Has --
	public bool Has( ITag tag )   => OfTagEnumeration( tag ).Any();
	public bool HasAny( params ITokenClass[] healthyInvaders ) => OfAnyTagEnumeration( healthyInvaders ).Any();

	// -- Sum --
	public int Sum( ITag tag ) => OfTagEnumeration( tag ).Sum( k => _counts[k] );
	public int SumAny( params ITag[] healthyInvaders ) => OfAnyTagEnumeration( healthyInvaders ).Sum( k => _counts[k] );

	// -- IEnumerable<SpaceToken> --
	public IEnumerable<SpaceToken> SpaceTokensOfTag( ITag tag ) => OfTagEnumeration(tag).On(Space);
	public IEnumerable<SpaceToken> SpaceTokensOfAnyTag( params ITag[] tag ) => OfAnyTagEnumeration( tag ).On(Space);

	// -- HumanToken[] --
	public IEnumerable<HumanToken> Humans() => OfType<HumanToken>();
	public HumanToken[] HumanOfTag( ITag tag ) => OfTagEnumeration( tag ).Cast<HumanToken>().ToArray();
	public HumanToken[] HumanOfAnyTag( params ITag[] classes ) => OfAnyTagEnumeration( classes ).Cast<HumanToken>().ToArray();

	// -- IToken[] --
	public IToken[] OfTag( ITag tag ) => OfTagEnumeration( tag ).ToArray();
	public IToken[] OfAnyTag( params ITag[] tags ) => OfAnyTagEnumeration( tags ).ToArray();

	// -- IEnumerable<IToken> --
	protected IEnumerable<IToken> OfTagEnumeration( ITag tag ) => OfType<IToken>().Where( token => token.HasTag(tag) );
	protected IEnumerable<IToken> OfAnyTagEnumeration( params ITag[] classes ) => OfType<IToken>().Where( token => token.HasAny(classes) );

	// .OfType<> 
	public IEnumerable<T> ModsOfType<T>() => Keys.Union( _islandMods ).OfType<T>();
	public IEnumerable<T> OfType<T>() => Keys.OfType<T>();

	// ==================
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
	public virtual TokenBinding Badlands => new ( this, Token.Badlands );
	public DahanBinding Dahan => new DahanBinding( this );
	public TokenBinding Vitality => new( this, Token.Vitality );
	public virtual InvaderBinding Invaders => new InvaderBinding( this );

	#endregion

	#region fields

	static void ValidateNotDead( ISpaceEntity specific ) {
		if(specific is HumanToken ht && ht.RemainingHealth == 0) 
			throw new ArgumentException( "We don't store dead counts" );
	}

	readonly CountDictionary<ISpaceEntity> _counts;
	readonly IEnumerable<ISpaceEntity> _islandMods;
	protected readonly IIslandTokenApi _api;

	#endregion

	#region Non-event Generationg Token Changes

	/// <summary> Non-event-triggering setup </summary>
	public void Adjust( ISpaceEntity specific, int delta ) {
		if(specific is HumanToken human && human.RemainingHealth == 0) 
			throw new System.ArgumentException( "Don't try to track dead tokens." );
		if(specific is ITrackMySpaces selfTracker) AdjustTrackedToken( selfTracker, delta );
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

	public IToken GetDefault( ITokenClass tokenClass ) => _api.GetDefault( tokenClass );

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
	public IEnumerable<HumanToken> InvaderTokens() => HumanOfTag( TokenCategory.Invader ).Cast<HumanToken>();

	public bool HasInvaders() => Has( TokenCategory.Invader );

	public bool HasStrife => Humans().Any(x=>0<x.StrifeCount);
	public int StrifeCount => Humans().Sum( x => x.StrifeCount );

	public int CountStrife() => Humans().Where(x=>0<x.StrifeCount).Sum( t => _counts[t] );

	public int TownsAndCitiesCount() => this.SumAny( Human.Town_City );

	public int InvaderTotal() => InvaderTokens().Sum( i => _counts[i] );

	#endregion

	// public int AttackDamageFrom1( HumanToken ht ) => ht.Attack;

	#region Adjacent Properties

	/// <summary> Space Adjacent_Existing (including gateway) </summary>
	public IEnumerable<SpaceState> Adjacent_Existing { 
		get {
			foreach(var space in Space.Adjacent_Existing)
				yield return space.Tokens;

			foreach(var gateway in OfType<GatewayToken>())
				yield return gateway.GetLinked(this);
		}
	}

	/// <summary> Existing & IsInPlay </summary>
	public IEnumerable<SpaceState> Adjacent => Adjacent_Existing.IsInPlay();

	public IEnumerable<SpaceState> Adjacent_ForInvaders => IsConnected ? Adjacent.Where( x => x.IsConnected ) : Enumerable.Empty<SpaceState>();

	public IEnumerable<SpaceState> Range(int maxDistance) => this.CalcDistances( maxDistance ).Keys.IsInPlay();

	/// <summary> Explicitly named so not to confuse with Powers - Range commands. </summary>
	public IEnumerable<SpaceState> InOrAdjacentTo => Range( 1 );

	/// <summary> Has no Isolate token. </summary>
	public bool IsConnected => !OfType<IIsolate>().Any(x=>x.IsIsolated);

	#endregion

	void AdjustTrackedToken( ITrackMySpaces token, int delta ) {
		token.TrackAdjust(Space,delta);
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

	public virtual async Task<SpaceToken> Add1StrifeToAsync( HumanToken invader ) => (await AddRemoveStrifeAsync( invader, 1, 1 )).On(Space);

	public Task<HumanToken> Remove1StrifeFromAsync( HumanToken invader, int tokenCount ) => AddRemoveStrifeAsync(invader,-1,tokenCount);

	/// <returns>New invader</returns>
	protected async Task<HumanToken> AddRemoveStrifeAsync( HumanToken originalInvader, int strifeDelta, int tokenCount ) {

		if(this[originalInvader] < tokenCount)
			throw new ArgumentOutOfRangeException( $"collection does not contain {tokenCount} {originalInvader}" );

		var newInvader = originalInvader.AddStrife( strifeDelta );
		// We need to generate events (for Observe the Ever changing World) so we will use the Replace reason
		await this.RemoveAsync( originalInvader, tokenCount, RemoveReason.Replaced );
		await AddAsync( newInvader, tokenCount, AddReason.AsReplacement );

		if(newInvader.IsDestroyed) // due to a strife-health penalty
			await Destroy( newInvader, this[newInvader] );

		return newInvader;
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

	public Task AddDefault( ITokenClass tokenClass, int count, AddReason addReason = AddReason.Added )
		=> AddAsync( GetDefault( tokenClass ), count, addReason );


	// Convenience only
	public Task Destroy( IToken token, int count ) => token is HumanToken ht
		? ht.Destroy( this, count )
		: this.RemoveAsync( token, count, RemoveReason.Destroyed );

	public async Task<ITokenAddedArgs> AddAsync( IToken token, int count, AddReason addReason = AddReason.Added ) {
		var (addResult,notifier) = await SinkAsync( token, count, addReason );
		if(addResult == null) return null;

		await notifier( addResult );

		return addResult;
	}

	/// <summary>
	/// Called by Add(...) and Move(...) to do the Adding... 
	/// DOES trigger the IModifyAdding handlers
	/// Does not trigger Add-Completed events.
	/// </summary>
	/// <returns>The move event, MAY contain Count=0</returns>
	public async Task<(ITokenAddedArgs,Func<ITokenAddedArgs,Task>)> SinkAsync( IToken token, int count, AddReason addReason = AddReason.Added ) {
		if(count < 0) throw new ArgumentOutOfRangeException( nameof( count ) );

		// Pre-Add check/adjust
		var addingArgs = new AddingTokenArgs( token, count, this, addReason );

		// Modify Adding
		await ModifyAdding( addingArgs );

		if(addingArgs.Count < 0) throw new IndexOutOfRangeException( nameof( addingArgs.Count ) );

		// Do Add
		if(0 < addingArgs.Count)
			Adjust( addingArgs.Token, addingArgs.Count );

		// Post-Add event
		return (
			new TokenAddedArgs( addingArgs.Token, this.Space, addingArgs.Count, addReason ),
			NotifyAddedAsync
		);
	}

	// callback used for ISinkTokens
	async Task NotifyAddedAsync( ITokenAddedArgs args ) {
		var tokens = ModSnapshot;
		// Sync
		foreach(IHandleTokenAdded handler in tokens.OfType<IHandleTokenAdded>())
			handler.HandleTokenAdded( this, args );
		// Async  (these must not be run in parallel because IDecision cannot handle it.)
		foreach(IHandleTokenAddedAsync handler in tokens.OfType<IHandleTokenAddedAsync>())
			await handler.HandleTokenAddedAsync( this, args );
	}

	/// <summary> Triggers IModifyRemoving but does NOT publish TokenRemovedArgs. </summary>
	public virtual async Task<(ITokenRemovedArgs,Func<ITokenRemovedArgs,Task>)> SourceAsync( IToken token, int count, RemoveReason reason = RemoveReason.Removed ) {
		count = System.Math.Min( count, this[token] );

		RemovingTokenCtx removedHandlers = RemovedHandlerSnapshop;

		// Pre-Remove check/adjust (sync)
		var removingArgs = new RemovingTokenArgs( this, reason ) { Count = count, Token = token };
		await ModifyRemoving( removingArgs );

		// Do Remove
		if(0<removingArgs.Count)
			Adjust( removingArgs.Token, -removingArgs.Count );

		// Post-Remove event
		return (
			new TokenRemovedArgs( Space, removingArgs.Token, removingArgs.Count, reason ),
			removedHandlers.NotifyRemoved
		);

	}

	#region Mods
	//-------------
	/// <summary> A snapshot of all tokens + the Island-mods </summary>
	ISpaceEntity[] ModSnapshot => Keys.Union(_islandMods).ToArray();

	async Task ModifyRemoving( RemovingTokenArgs args ) {
		var modArray = ModSnapshot;
		// Sync
		foreach(IModifyRemovingToken mod in modArray.OfType<IModifyRemovingToken>())
			if(0 < args.Count)
				mod.ModifyRemoving( args );
		// Async - (must NOT do this in Parallel)
		foreach(IModifyRemovingTokenAsync x in modArray.OfType<IModifyRemovingTokenAsync>())
			if(0 < args.Count)
				await x.ModifyRemovingAsync( args );
	}

	protected RemovingTokenCtx RemovedHandlerSnapshop => new RemovingTokenCtx( this, ModSnapshot );

	async Task ModifyAdding( AddingTokenArgs args ) {
		var keyArray = ModSnapshot; 
		foreach(var mod in keyArray.OfType<IModifyAddingToken>()) 
			if(0 < args.Count)
				mod.ModifyAdding( args );
		// Sync - series, NOT parallel (IDecision can't manage them)
		foreach(var mod in keyArray.OfType<IModifyAddingTokenAsync>())
			if(0 < args.Count)
				await mod.ModifyAddingAsync( args );
	}

	public void TimePasses() {
		var keyArray = ModSnapshot;
		foreach(var cleanup in keyArray.OfType<ISpaceEntityWithEndOfRoundCleanup>())
			cleanup.EndOfRoundCleanup( this );

		// remove keys (this-space-only, no entities from Island Mods)
		foreach(var removeMe in OfType<IEndWhenTimePasses>().ToArray())
			Init(removeMe,0);
	}

	//-------------
	#endregion Mods

	public virtual HumanToken GetNewDamagedToken( HumanToken invaderToken, int availableDamage )
		=> invaderToken.AddDamage( availableDamage );

	protected virtual Task<int> DestroyNInvaders( HumanToken invaderToDestroy, int countToDestroy ) {
		return invaderToDestroy.Destroy( this, countToDestroy );
	}

	public virtual TokenMover Gather( Spirit self ) 
		=> new TokenMover( self, "Gather", Adjacent, this );

	public virtual TokenMover Pusher( Spirit self, SourceSelector sourceSelector, DestinationSelector dest = null ) 
		=> new TokenMover( self, "Push", sourceSelector, dest ?? PushDestinations );

	public virtual DestinationSelector PushDestinations => DestinationSelector.Adjacent;

	#region Ravage

	/// <summary> Does 1 potential Ravage (if no stopper tokens) </summary>
	public Task Ravage() => RavageBehavior.Exec( this );

	public RavageBehavior RavageBehavior {
		get {
			var mod = Keys.OfType<RavageBehavior>().FirstOrDefault();
			if(mod == null) {
				mod = RavageBehavior.DefaultBehavior.Clone();
				Init( mod, 1 );
			}
			return mod;
		}
	}

	#endregion

	public virtual async Task DestroySpace() {

		// Destroy Invaders
		await Invaders.DestroyAll( Human.Invader );

		// Destroy Dahan
		await Dahan.DestroyAll();

		// Blight is removed from the game and does not go back to the card
		Blight.Init(0);

		// Destroy all other tokens
		foreach(IToken token in OfType<IToken>().ToArray())
			await Destroy( token, this[token] );

		if(Space is Space1 s1)
			s1.NativeTerrain = Terrain.Destroyed;
		else if(Space is MultiSpace ms)
			foreach(var part in ms.OrigSpaces)
				part.NativeTerrain = Terrain.Destroyed;

	}

	public void Isolate() => Init(Token.Isolate,1);

	// Helper
	public SourceSelector SourceSelector => new SourceSelector(this);

}

/// <summary>
/// Captures the Mod tokens before they are removed, so their handlers can be invoked post-removal
/// </summary>
public class RemovingTokenCtx {
	readonly SpaceState _from;
	readonly ISpaceEntity[] _keyArray;
	public RemovingTokenCtx( SpaceState from, ISpaceEntity[] keyArray ) { 
		_from = from;
		_keyArray = keyArray; 
	}
	public async Task NotifyRemoved( ITokenRemovedArgs args ) {
		// Sync
		foreach(IHandleTokenRemoved handler in _keyArray.OfType<IHandleTokenRemoved>())
			handler.HandleTokenRemoved( _from, args );
		// Async
		foreach(IHandleTokenRemovedAsync x in _keyArray.OfType<IHandleTokenRemovedAsync>())
			await x.HandleTokenRemovedAsync( _from, args );
	}
}
