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
			// We do temporarily store Destroyed, then immediatly Remove/Destroy them
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
	public IEnumerable<HumanToken> AllHumanTokens() => OfType<HumanToken>();
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
	public TokenBinding Disease => new ( this, Token.Disease );
	public TokenBinding Wilds => new ( this, Token.Wilds );
	public virtual TokenBinding Badlands => new ( this, Token.Badlands );
	public DahanBinding Dahan => new DahanBinding( this );
	public TokenBinding Vitality => new( this, Token.Vitality );
	public virtual InvaderBinding Invaders => new InvaderBinding( this );

	#endregion

	#region fields

	static void ValidateNotDestroyed( ISpaceEntity specific ) {
		if(specific is HumanToken ht && ht.IsDestroyed) 
			throw new ArgumentException( "We don't store dead counts" );
	}

	readonly CountDictionary<ISpaceEntity> _counts;
	readonly IEnumerable<ISpaceEntity> _islandMods;
	protected readonly IIslandTokenApi _api;

    #endregion

    #region Non-event Generationg Token Changes

	/// <summary> Add or Remove tokens without generating events. </summary>
	public void Adjust( IToken specific, int delta ) {
		// Don't let them add Destroyed tokens.
		// But it is ok to remove destroyed tokens because we might be undoing a destroyed token
		if( 0 < delta ) ValidateNotDestroyed( specific );

        if(specific is ITrackMySpaces selfTracker) AdjustTrackedToken( selfTracker, delta );
        _counts[specific] += delta;
    }

	/// <summary>
	/// For Adjust()ing in the positive direction, HumanTokens that might be Destroyed. 
	/// Only trigger a Destroyed/Remove event if the added token is already destroyed.
	/// </summary>
	/// <remarks> Though counter-intuitive, this significantly simplifies replacing-token logic.</remarks>
	public async Task AdjustUpOrDestroyAsync( HumanToken token, int deltaCount ) {
		if(deltaCount < 0) throw new ArgumentOutOfRangeException(nameof(deltaCount), "Add only. For removing tokens, use Adjust(...)" );
		if(deltaCount == 0) return;

		// Do Adjustment
		if(token is ITrackMySpaces selfTracker) AdjustTrackedToken( selfTracker, deltaCount );
		_counts[token] += deltaCount;

		if(token.IsDestroyed) {
			ActionScope.Current.LogDebug( $"{Space.Text} Adjusting-Up {deltaCount} {token.SpaceAbreviation} => Destroyed!" );
			await this.RemoveAsync(token, deltaCount, RemoveReason.Destroyed);
		}
	}

	/// <summary> Non-event-triggering changes </summary>
	public void Adjust( ISpaceEntity specific, int delta ) {
		if(specific is HumanToken human && human.RemainingHealth == 0) 
			throw new System.ArgumentException( "Don't try to track dead tokens." );
		if(specific is ITrackMySpaces selfTracker) AdjustTrackedToken( selfTracker, delta );
		_counts[specific] += delta;
	}

	/// <summary> Non-event setup. </summary>
    public void Init( ISpaceEntity specific, int newValue ) {
		int old = _counts[specific];
		Adjust( specific, newValue-old ); // go through Adjust so that we keep ITrackMySpaces in sync
	}

	public void InitDefault( HumanTokenClass tokenClass, int value )
		=> Init( GetDefault( tokenClass ), value );

	public void AdjustDefault( HumanTokenClass tokenClass, int delta ) 
		=> Adjust( GetDefault( tokenClass ), delta );

	public HumanToken GetDefault( ITokenClass tokenClass ) => _api.GetDefault( tokenClass );

	#endregion

	#region AdjustProps

	public HumanAdjustBinding Humans( int count, HumanToken tokenToReplace ) 
		=> new HumanAdjustBinding( this, count, tokenToReplace );

	public HumanAdjustBinding AllHumans( HumanToken orig ) => Humans( this[orig], orig );

	#endregion AdjustProps

	#region Invader Specific

	/// <summary> Includes dreaming invaders. </summary>
	public IEnumerable<HumanToken> InvaderTokens() => HumanOfTag( TokenCategory.Invader ).Cast<HumanToken>();

	public bool HasInvaders() => Has( TokenCategory.Invader );

	public bool HasStrife => AllHumanTokens().Any(x=>0<x.StrifeCount);
	public int StrifeCount => AllHumanTokens().Sum( x => x.StrifeCount );

	public int CountStrife() => AllHumanTokens().Where(x=>0<x.StrifeCount).Sum( t => _counts[t] );

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

		var replacement = await ReplaceAsync( originalInvader, tokenCount, originalInvader.AddStrife(strifeDelta) );

		return replacement.Added.AsHuman();
	}

	/// <summary>
	/// Replaces 1 Human type/class with another (1 to 1)
	/// </summary>
	/// <returns>null IF oldToken is null.</returns>
	public async Task<TokenReplacedArgs> ReplaceHumanAsync(HumanToken oldToken, HumanTokenClass newTokenClass) {
		if(oldToken == null) return null;

		var newToken = GetDefault( newTokenClass );

		newToken = newToken.AddDamage( oldToken.Damage, oldToken.DreamDamage );
		if(newToken.HasTag(TokenCategory.Invader))
			newToken = newToken.AddStrife(oldToken.StrifeCount);

		// if downgrading it, destroys it, then do nothing
		if(newToken.IsDestroyed) {
			// Invaders
			if( newToken.HasTag( TokenCategory.Invader ) && PreventsInvaderDamage() ) return TokenReplacedArgs.Null(this,oldToken,newToken);
			// Dahan
		}

		return await ReplaceAsync( oldToken, 1, newToken );
	}

	/// <returns>null if null oldToken passed in.</returns>
	public async Task<TokenReplacedArgs> ReplaceAsync(IToken oldToken, int newCount, IToken newToken) {
		if(oldToken == null) return null;

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
		ILocation source = this;
#pragma warning restore CA1859 // Use concrete types when possible for improved performance

		var (removed, removedHandler) = await source.SourceAsync( oldToken, 1, RemoveReason.Replaced );
		if(removed.Count == 0) return TokenReplacedArgs.Null(this,oldToken,newToken);

		var (added, addedHandler) = await SinkAsync( newToken, newCount, AddReason.AsReplacement);

		var replaced = new TokenReplacedArgs(removed,added);

		await removedHandler( replaced );
		await addedHandler( replaced );

		ActionScope.Current.Log(replaced);
		return replaced;
	}

	public Task<ITokenAddedArgs> AddDefaultAsync( ITokenClass tokenClass, int count, AddReason addReason = AddReason.Added )
		=> AddAsync( GetDefault(tokenClass), count, addReason);

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
		ArgumentOutOfRangeException.ThrowIfNegative( count );

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

	public Task<int> DestroyAll( HumanToken humanToken )
		=> Destroy( humanToken, this[humanToken] );

	protected virtual Task<int> DestroyNInvaders( HumanToken invaderToDestroy, int countToDestroy ) {
		return this.Destroy( invaderToDestroy, countToDestroy );
	}

	// Convenience only
	public async Task<int> Destroy( IToken token, int count ) {
		if(this[token] < count)
			throw new InvalidOperationException( $"Cannot remove {count} {this} tokens because there aren't that many." );

		var result = await this.RemoveAsync( token, count, RemoveReason.Destroyed );
		if(token is HumanToken ht )
			this.AddFear(
				ht.HumanClass.FearGeneratedWhenDestroyed * result.Count,
				FearType.FromInvaderDestruction // this is the destruction that Dread Apparitions ignores.
			);
		return result.Count;
	}

	/// <summary> Triggers IModifyRemoving but does NOT publish TokenRemovedArgs. </summary>
	public virtual async Task<(ITokenRemovedArgs,Func<ITokenRemovedArgs,Task>)> SourceAsync( IToken token, int count, RemoveReason reason = RemoveReason.Removed ) {
		count = System.Math.Min( count, this[token] );

		RemovingTokenCtx removedHandlers = RemovedHandlerSnapshop;

		// Pre-Remove check/adjust (sync)
		RemovingTokenArgs removingArgs = reason == DestroyingFromDamage.TriggerReason
			? new DestroyingFromDamage( this ) { Count = count, Token = token }
			: new RemovingTokenArgs( this, reason ) { Count = count, Token = token };

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

	public bool PreventsInvaderDamage() => ModsOfType<IStopInvaderDamage>().Any();

}

/// <summary>
/// Captures the Mod tokens before they are removed, so their handlers can be invoked post-removal
/// </summary>
public class RemovingTokenCtx( SpaceState from, ISpaceEntity[] keyArray ) {
	public async Task NotifyRemoved( ITokenRemovedArgs args ) {
		// Sync
		foreach(IHandleTokenRemoved handler in keyArray.OfType<IHandleTokenRemoved>())
			handler.HandleTokenRemoved( from, args );
		// Async
		foreach(IHandleTokenRemovedAsync x in keyArray.OfType<IHandleTokenRemovedAsync>())
			await x.HandleTokenRemovedAsync( from, args );
	}
}
