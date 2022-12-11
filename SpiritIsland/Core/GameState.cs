namespace SpiritIsland;

public class GameState : IHaveHealthPenaltyPerStrife {

	#region constructors

	/// <summary>
	/// Simplified constructor for single-player
	/// </summary>
	public GameState( Spirit spirit, Board board ) : this(spirit) {
		this.Island = new Island(board);
	}

	public GameState(params Spirit[] spirits){
		if(spirits.Length==0) throw new ArgumentException("Game must include at least 1 spirit");
		this.Spirits = spirits;

		// Note: don't init invader deck here, let users substitute
		RoundNumber = 1;
		Fear = new Fear( this );
		Invaders = new Invaders( this );
		Tokens = new Tokens_ForIsland( this );

		Tokens.TokenAdded.ForGame.Add( BlightAddedCheck );
		Tokens.TokenRemoved.ForGame.Add( BlightRemovedCheck );

		TimePasses_WholeGame += TokenCleanUp;
		TimePasses_WholeGame += ModifyBlightAddedEffect.ForRound.Clear;
		TimePasses_WholeGame += EndOfAction.ForRound.Clear;
	}

	/// <summary>
	/// Called AFTER everything has been configured. and BEFORE players make first move.
	/// </summary>
	public void Initialize() {
		PlaceStartingTokens(); 
		InitialExplore();
		InitSpirits();// ManyMinds requires the beast to be in place, so this goes after tokens are placed.
		BlightCard.OnGameStart( this );
	}

	void InitialExplore() {
		InvaderDeck.InitExploreSlot();
		InvaderDeck.Explore.Cards[0].Explore( this ).Wait();
		InvaderDeck.Advance();
	}

	void PlaceStartingTokens() {
		foreach(var board in Island.Boards) {
			Tokens[board[2]].Disease.Init( 1 );
			var lowest = board.Spaces.Skip( 1 ).OfType<Space1>().First( s => s.StartUpCounts.Empty );
			Tokens[lowest].Beasts.Adjust( 1 );
		}

		foreach(var board in Island.Boards)
			foreach(var space in board.Spaces)
				((Space1)space).InitTokens( Tokens[space] );
	}

	/// <summary>  Calls each spirit's InitSpirit(board,gameState) method </summary>
	void InitSpirits() {
		if(Spirits.Length != Island.Boards.Length)
			throw new InvalidOperationException( "# of spirits and islands must match" );
		for(int i = 0; i < Spirits.Length; ++i)
			Spirits[i].InitSpirit( Island.Boards[i], this );
	}

	#endregion

	// base-1,  game starts in round-1
	public int RoundNumber { get; private set; }
	public Phase Phase { get; set; }

	// == Components ==
	public Fear Fear { get; }
	public Island Island { get; set; }
	public Spirit[] Spirits { get; }

	public Tokens_ForIsland Tokens { get; }
	public IEnumerable<SpaceState> AllSpaces => Island.Boards
		.SelectMany(b=>b.Spaces)
		.Select(Tokens.GetTokensFor);
	public IEnumerable<SpaceState> AllActiveSpaces => AllSpaces.Where( t => !t.InStasis );

	public PowerCardDeck MajorCards {get; set; }
	public PowerCardDeck MinorCards { get; set; }
	public InvaderDeck InvaderDeck { 
		get { return _invaderDeck ??= new InvaderDeck(); }
		set { _invaderDeck = value; }
	}
	InvaderDeck _invaderDeck;
	public Invaders Invaders { get; } // creates ravage/damage objects - Obsolete - just make Tokens do this.
	public int blightOnCard; // 2 per player + 1
	public IBlightCard BlightCard = new NullBlightCard();
	public List<IBlightCard> BlightCards = new List<IBlightCard>();
	public GameOver Result = null;
	public int HealthPenaltyPerStrife { get; set; } = 0;

	#region Blight

	public int DamageToBlightLand = 2;

	public async Task DamageLandFromRavage( Space space, int damageInflictedFromInvaders, UnitOfWork actionId ) {
		if(damageInflictedFromInvaders==0) return;

		await LandDamaged.InvokeAsync( new LandDamagedArgs { 
			GameState = this,
			Space = space,
			Damage = damageInflictedFromInvaders,
			ActionId = actionId
		} );

		if( damageInflictedFromInvaders >= DamageToBlightLand)
			await Tokens[space].Blight.Bind(actionId).Add(1, AddReason.Ravage);
	}

	/// <summary>
	/// Does all the special actions when blight is added.
	/// </summary>
	async Task BlightAddedCheck( ITokenAddedArgs args ){
		if(args.Token != TokenType.Blight) return; // token-added event handler for blight only

		bool isCascading = args.Reason switch {
			AddReason.AsReplacement  => false,
			AddReason.MovedTo        => false,
			AddReason.Added          => true, // Generic add
			AddReason.Ravage         => true, // blight from ravage
			AddReason.BlightedIsland => true, // blight from blighted island card
			AddReason.SpecialRule    => true, // Heart of wildfire - Blight from add presence
			_ => throw new ArgumentException(nameof(args.Reason))
		};
		if( !isCascading ) return;

		// remove from card.
		await TakeFromBlightSouce( args.Count, args.Space );

		if(BlightCard != null && blightOnCard <= 0) {
			Log( new IslandBlighted( BlightCard ) );
			await BlightCard.OnBlightDepleated( this );
		}

		// Calc side effects
		bool isFirstBlight = args.Space.Blight.Count == 1;
		var effect = new AddBlightEffect { DestroyPresence = true, Cascade = !isFirstBlight, GameState = this, Space = args.Space };
		await ModifyBlightAddedEffect.InvokeAsync(effect);

		// Destory presence
		if(effect.DestroyPresence)
			foreach(var spirit in Spirits)
				if(spirit.Presence.IsOn( args.Space ))
					await spirit.Presence.Destroy( args.Space.Space, this, DestoryPresenceCause.Blight, args.Action, args.Reason );

		// Cascade blight
		if(effect.Cascade) {
			Space cascadeTo = await Spirits[0].Gateway.Decision( Select.Space.ForAdjacent(
				$"Cascade blight from {args.Space.Space.Label} to",
				args.Space.Space,
				Select.AdjacentDirection.Outgoing,
				args.Space.CascadingBlightOptions,
				Present.Always
			));
			await Tokens[ cascadeTo ].Blight.Bind(args.Action).Add(1, args.Reason); // Cascading blight shares original blights reason.
		}

	}

	/// <summary>
	/// Event handler for token removed that checks blight-only
	/// </summary>
	void BlightRemovedCheck( ITokenRemovedArgs args ) {
		if(args.Token == TokenType.Blight
			&& !args.Reason.IsOneOf(
				RemoveReason.MovedFrom, // pushing / gathering blight
				RemoveReason.Replaced   // just in case...
			)
		)
			this.blightOnCard += args.Count;
	}

	#endregion

	#region Skip Invader Actions

	public void SkipAllInvaderActions( Space target, string label ) {
		// Sometimes this will be called with nothing to skip, Example: Quarentine level 3
		Skip1Ravage( target, label );
		AdjustTempToken( target, SkipBuild.Default( label ) );
		Skip1Explore( Tokens[target], label );
	}

	public void Skip1Ravage( Space space, string label, Func<GameState, SpaceState, Task> altAction = null ) {
		AdjustTempToken( space, new SkipRavage(label) );
	}

	public void Skip1Build( Space space, string label ) {
		AdjustTempToken( space, SkipBuild.Default( label ) );
	}

	public void Skip1Explore( SpaceState space, string label ) {
		space.Adjust( new SkipExploreTo( label ), 1 );
	}

	public void SkipAllBuilds( Space space, string label, params TokenClass[] stoppedClasses  ) {
		if( stoppedClasses == null || stoppedClasses.Length == 0 )
			stoppedClasses = new TokenClass[] { Invader.Town, Invader.City };
		AdjustTempToken( space, new SkipBuild( label, UsageDuration.SkipAllThisTurn, stoppedClasses ));
	}

	public void AdjustTempTokenForAll( Token token ) {
		foreach(var space in AllActiveSpaces)
			AdjustTempToken( space.Space, token );
	}

	/// <summary> Adds 1 temporary tokens that remove themselves at Time-Passes. </summary>
	/// <remarks> For not-real tokens like Explore/Build/Ravage Stoppers. </remarks>
	public void AdjustTempToken( Space space, Token token ) {
		// Add it
		Tokens[space].Adjust( token, 1 );
		// Remove it at end of turn
		TimePasses_ThisRound.Push( gs => {
			var tokens = gs.Tokens[space];
			if(tokens[token] > 0)
				tokens.Adjust( token, -1 );
			return Task.CompletedTask;
		} );
	}
	#endregion

	#region Pour Time Sideways - Add Invader actions

	public void PourTimeSideways_Add1Ravage( Space spacesToAdd ) {
		throw new System.NotImplementedException( "!!! should only add to cards that match space" );
	}

	public void PourTimeSideways_Add1Build( params Space[] target ) {
		// !!! instead, call Invader build card twice.
		var buildCard = InvaderDeck.Build.Cards.FirstOrDefault();
		if(buildCard == null) return;
		foreach(var space in target.Where(buildCard.MatchesCard).ToArray())
			Tokens[space].Adjust( TokenType.DoBuild, 1 );
	}

	public void PourTimeSideways_Add1Explore( params Space[] target ) {
		// !!! This should only add to spaces that match invader card
		var exploreCard = InvaderDeck.Explore.Cards.FirstOrDefault();
		if(exploreCard == null) return;
		foreach(var space in target.Where( exploreCard.MatchesCard ).ToArray())
			Tokens[space].Adjust( TokenType.DoExplore, 1 );
	}

	#endregion Pour Time Sideways - Add Invader actions

	#region Configure Ravage

	public ConfigureRavage GetRavageConfiguration( Space space ) => _ravageConfig.ContainsKey( space ) ? _ravageConfig[space] : new ConfigureRavage();

	public void ModifyRavage( Space space, Action<ConfigureRavage> action ) {
		if(!_ravageConfig.ContainsKey( space ))
			_ravageConfig.Add( space, new ConfigureRavage() );
		action( _ravageConfig[space] );
	}

	readonly Dictionary<Space, ConfigureRavage> _ravageConfig = new Dictionary<Space, ConfigureRavage>(); // change ravage state of a Space

	#endregion

	public DahanGroupBindingNoEvents DahanOn( Space space ) => Tokens[space].Dahan; // Obsolete - use TargetSpaceCtx

	#region API - overridable

	public DualAsyncEvent<AddBlightEffect> ModifyBlightAddedEffect = new DualAsyncEvent<AddBlightEffect>();

	public Func<int, SpaceState,Task> TakeFromBlightSouce {
		get { return _takeFromBlightSouce ?? Default_TakeBlightFromSource; }
		set { _takeFromBlightSouce = value; }
	}
	Func<int, SpaceState, Task> _takeFromBlightSouce;

	public Func<Spirit,GameState,Cause,UnitOfWork,Task> Destroy1PresenceFromBlightCard = DefaultDestroy1PresenceFromBlightCard; // Direct distruction from Blight Card, not cascading

	void TokenCleanUp( GameState gs ) { 
		Healer.HealAll( gs ); // called at end of round.
		foreach(var spaceTokens in AllSpaces)
			spaceTokens.Blight.Blocked = false;
	}
	public Healer Healer = new Healer(); // replacable Behavior

	/// <param name="cat">Has no functional use.  Just helps us keep straight in our head what kind of action this is.</param>
	public UnitOfWork StartAction(ActionCategory cat) => new UnitOfWork( EndOfAction, cat );

	#endregion

	#region Win / Loss

	readonly List<Action> WinLossChecks = new List<Action>();
	public bool ShouldCheckWinLoss {
		set {
			WinLossChecks.Clear();
			if(value)
				WinLossChecks.Add(CheckTerrorLevelVictory);
		}
	}

	public void CheckWinLoss() {
		foreach(var check in WinLossChecks)
			check();
	}

	// Win Loss Predicates
	static (Func<SpaceState,bool>,string) InvaderCriteria(GameState gs) {
		bool NoCity(SpaceState space) => space.Sum(Invader.City)==0;
		bool NoCityOrTown(SpaceState space) => space.SumAny(Invader.City,Invader.Town)==0;
		bool NoInvader(SpaceState space) => !space.HasInvaders();
		return gs.Fear.TerrorLevel switch {
			4 => (_=>true, "Victory"),
			3 => (NoCity,"no cities"),
			2 => (NoCityOrTown,"no towns or cities"),
			_ => (NoInvader,"no invaders")
		};
	}

	// ! This is the only thing that needs checked after every action.
	void CheckTerrorLevelVictory(){
		var (filter,description) = InvaderCriteria(this);
		if( AllSpaces.All( filter ) )
			GameOverException.Win($"Terror Level {Fear.TerrorLevel} - {description}");
	}

	#endregion

	#region Default API methods

	/// <returns># of blight to remove from card</returns>
	Task Default_TakeBlightFromSource( int count, SpaceState _ ) {
		if( count < 0 ) throw new ArgumentOutOfRangeException(nameof(count));
		this.blightOnCard -= count;
		return Task.CompletedTask;
	}

	static async Task DefaultDestroy1PresenceFromBlightCard( Spirit spirit, GameState gs, Cause _, UnitOfWork action ) {
		var boundPresence = new ReadOnlyBoundPresence( spirit, gs, gs.Island.Terrain_ForBlight );
		var presenceSpace = await spirit.Gateway.Decision( Select.DeployedPresence.ToDestroy( "Blighted Island: Select presence to destroy.", boundPresence ) );
		await spirit.Presence.Destroy( presenceSpace, gs, DestoryPresenceCause.BlightedIsland, action );
	}

	#endregion

	public void Log( ILogEntry entry ) => NewLogEntry?.Invoke( entry );

	public async Task TriggerTimePasses() {

		_ravageConfig.Clear();

		// Clear Defend
		foreach(var s in AllSpaces)
			s.Defend.Clear();

		// Do Custom end-of-round cleanup stuff before round switches over
		// (shifting memory need cards it is going to forget to still be in hand when calling .Forget() on it)
		while(TimePasses_ThisRound.Count > 0)
			await TimePasses_ThisRound.Pop()( this );

		// Do the standard round-switch-over stuff.
		TimePasses_WholeGame?.Invoke( this );
		++RoundNumber;
	}

	#region Events

	// - Events -
	public event Action<ILogEntry> NewLogEntry;
	public DualAsyncEvent<GameState> StartOfInvaderPhase   = new DualAsyncEvent<GameState>();          // Blight effects
	public DualAsyncEvent<LandDamagedArgs> LandDamaged     = new DualAsyncEvent<LandDamagedArgs>();    // Let Them Break Themselves Against the Stone
	public DualAsyncEvent<UnitOfWork> EndOfAction          = new DualAsyncEvent<UnitOfWork>();

	public event Action<GameState> TimePasses_WholeGame;                                               // Spirit cleanup
	public Stack<Func<GameState, Task>> TimePasses_ThisRound = new Stack<Func<GameState, Task>>();     // This must be Push / Pop

	#endregion

	#region Game-Wide overrideable Behavior

	public Func<GameCtx,SpaceState,TokenClass,Task<bool>> Disease_StopBuildBehavior = Disease_StopBuildBehavior_Default;
	static async Task<bool> Disease_StopBuildBehavior_Default( GameCtx ctx, SpaceState tokens, TokenClass _ ) {
		await tokens.Disease.Bind( ctx.UnitOfWork ).Remove( 1, RemoveReason.UsedUp );
		return true;
	}

	public BuildEngine GetBuildEngine( SpaceState tokens ) => new BuildEngine( this, tokens );

	#endregion overrideable Game-Wide Behavior

	#region Memento

	public virtual IMemento<GameState> SaveToMemento() => new Memento(this);
	public virtual void LoadFrom( IMemento<GameState> memento ) => ((Memento)memento).Restore(this);

	protected class Memento : IMemento<GameState> {
		public Memento(GameState src) {
			roundNumber  = src.RoundNumber;
			blightOnCard = src.blightOnCard;
			isBlighted   = src.BlightCard.CardFlipped;
			spirits      = src.Spirits.Select(s=>s.SaveToMemento()).ToArray();
			if(src.MajorCards != null) major = src.MajorCards.SaveToMemento();
			if(src.MinorCards != null) minor = src.MinorCards.SaveToMemento();
			invaderDeck  = src.InvaderDeck.SaveToMemento();
			fear         = src.Fear.SaveToMemento();
			tokens       = src.Tokens.SaveToMemento();
			startOfInvaderPhase = src.StartOfInvaderPhase.SaveToMemento();
		}
		public void Restore(GameState src ) {
			src.RoundNumber = roundNumber;
			src.blightOnCard = blightOnCard;
			src.BlightCard.CardFlipped = isBlighted;
			for(int i=0;i<spirits.Length;++i) src.Spirits[i].LoadFrom( spirits[i] );
			if(src.MajorCards != null ) src.MajorCards.RestoreFrom( major );
			if(src.MinorCards != null ) src.MinorCards.RestoreFrom( minor );
			src.InvaderDeck.LoadFrom( invaderDeck );
			src.Fear.LoadFrom( fear );
			src.Tokens.LoadFrom( tokens );
			src.StartOfInvaderPhase.LoadFrom( startOfInvaderPhase );
		}
		readonly int roundNumber;
		readonly int blightOnCard;
		readonly bool isBlighted;
		readonly IMemento<Spirit>[] spirits;
		readonly IMemento<PowerCardDeck> major;
		readonly IMemento<PowerCardDeck> minor;
		readonly IMemento<InvaderDeck> invaderDeck;
		readonly IMemento<Fear> fear;
		readonly IMemento<Tokens_ForIsland> tokens;
		readonly IMemento<DualAsyncEvent<GameState>> startOfInvaderPhase;
	}

	#endregion Memento

}

public class Healer {

	public virtual void HealAll( GameState gs ) {
		foreach(var space in gs.Tokens.Keys)
			HealSpace( gs.Tokens[space] );
		skipHealSpaces.Clear();
	}

	public virtual void HealSpace( SpaceState tokens ) {
		if( !skipHealSpaces.Contains(tokens.Space) )
			InvaderBinding.HealTokens( tokens );
	}

	public void Skip( Space space ) => skipHealSpaces.Add( space );

	protected HashSet<Space> skipHealSpaces = new HashSet<Space>();

}

public class LandDamagedArgs {
	public GameState GameState;
	public Space Space;
	public int Damage;
	public UnitOfWork ActionId;
}

public class AddBlightEffect {
	public bool Cascade;
	public bool DestroyPresence;

	public GameState GameState;
	public SpaceState Space;
}