namespace SpiritIsland;

public class GameState {

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

		TimePasses_WholeGame += Heal;
		TimePasses_WholeGame += PreRavaging.ForRound.Clear;
		TimePasses_WholeGame += PreBuilding.ForRound.Clear;
		TimePasses_WholeGame += PreExplore.ForRound.Clear;
	}

	/// <summary>
	/// Called AFTER everything has been configured. and BEFORE players make first move.
	/// </summary>
	public void Initialize() {

		// ! this has to go first since ManyMinds requires the beast to be in place
		foreach(var board in Island.Boards) {
			Tokens[board[2]].Disease.Init(1);
			var lowest = board.Spaces.Skip(1).OfType<Space1>().First(s=>s.StartUpCounts.Empty);
			Tokens[lowest].Beasts.Adjust(1);
		}

		foreach(var board in Island.Boards)
			foreach(var space in board.Spaces)
				((Space1)space).InitTokens( Tokens[space] );

		// Explore
		InvaderDeck.Explore[0].Explore( this ).Wait();

		InvaderDeck.Advance();

		InitSpirits();

		// Blight
		BlightCard.OnGameStart( this );
		Tokens.TokenRemoved.ForGame.Add( args => {
			if(args.Token == TokenType.Blight)
				this.blightOnCard += args.Count;
		} );
		Tokens.TokenAdded.ForGame.Add( async args => {
			if(args.Token == TokenType.Blight)
				await BlightAdded( args.Space, args.Count );
		} );
	}

	void InitSpirits() {
		if(Spirits.Length != Island.Boards.Length)
			throw new InvalidOperationException( "# of spirits and islands must match" );
		for(int i = 0; i < Spirits.Length; ++i)
			Spirits[i].Initialize( Island.Boards[i], this );
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
	public PowerCardDeck MajorCards {get; set; }
	public PowerCardDeck MinorCards { get; set; }
	public InvaderDeck InvaderDeck { 
		get { return _invaderDeck ??= new InvaderDeck(new Random()); }
		set { _invaderDeck = value; }
	}
	InvaderDeck _invaderDeck;
	public Invaders Invaders { get; } // creates ravage/damage objects - Obsolete - just make Tokens do this.
	public int blightOnCard; // 2 per player + 1
	public IBlightCard BlightCard = new NullBlightCard();
	public List<IBlightCard> BlightCards = new List<IBlightCard>();
	public GameOver Result = null;

	#region Blight

	public int DamageToBlightLand = 2;

	public async Task DamageLandFromRavage( Space space, int damageInflictedFromInvaders ) {
		if(damageInflictedFromInvaders==0) return;

		await LandDamaged.InvokeAsync( new LandDamagedArgs { GameState = this, Space = space, Damage = damageInflictedFromInvaders} );

		if( damageInflictedFromInvaders >= DamageToBlightLand)
			await Tokens[space].Blight.Add(1, AddReason.TakenFromCard); // !!! remove .TakenFromCard 
	}

	async Task BlightAdded( Space blightSpace, int count ){

		// remove from card.
		TakeFromBlightSouce( count, blightSpace );

		if(BlightCard != null && blightOnCard <= 0) {
			Log( new IslandBlighted( BlightCard ) );
			await BlightCard.OnBlightDepleated( this );
		}

		// Calc side effects
		var effect = AddBlightSideEffect( this, blightSpace );

		// Destory presence
		if(effect.DestroyPresence)
			foreach(var spirit in Spirits)
				if(spirit.Presence.IsOn( blightSpace ))
					await spirit.Presence.Destroy( blightSpace, this, ActionType.Invader );

		// Cascade blight
		if(effect.Cascade) {
			Space cascadeTo = await Spirits[0].Action.Decision( Select.Space.ForAdjacent(
				$"Cascade blight from {blightSpace.Label} to",
				blightSpace,
				Select.AdjacentDirection.Outgoing,
				blightSpace.Adjacent.Where( x => !Island.TerrainMapFor( Cause.Blight ).IsOneOf( x, Terrain.Ocean ) ),
				Present.Always
			));
			await Tokens[ cascadeTo ].Blight.Add(1,AddReason.TakenFromCard);
		}

	}

	#endregion

	#region Invader Phase / Deck Modifications

	public void SkipAllInvaderActions( params Space[] targets ) {
		if(targets == null || targets.Length == 0)
			throw new ArgumentException("must provide targets to skip");

		foreach(var target in targets){
			SkipRavage( target );
			Skip1Build( target );
			SkipExplore( target );
		}
	}

	public void SkipRavage( Space space, Func<GameState,Space,Task> altAction = null ) {
		PreRavaging.ForRound.Add( async ( args ) => {
			args.Skip1(space);
			if(altAction != null)
				await altAction( this, space );
		} );
	}

	public void AddRavage( Space spacesToAdd ) {
		throw new System.NotImplementedException("!!! should only add to cards that match space");
	}


	public void Skip1Build( Space space, Func<GameState,Space,Task> altAction = null ) {
		PreBuilding.ForRound.Add( async (args) => {
			args.Skip1(space);
			if(altAction != null)
				await altAction( this, space );
		} );
	}

	public void Add1Build( params Space[] target ) {
		// !!! This should only add to spaces that match invader card
		PreBuilding.ForRound.Add( ( BuildingEventArgs args ) => {
			foreach(var space in target)
				args.Add( space );
		} );
	}


	public void SkipExplore( Space space, Func<GameState,Space,Task> altAction = null  ) {
		PreExplore.ForRound.Add( async ( args ) => {
			args.Skip(space);
			if(altAction != null)
				await altAction( this, space );
		} );
	}

	public void AddExplore( params Space[] target ) {
		// !!! This should only add to spaces that match invader card
		PreExplore.ForRound.Add( ( args ) => {
			foreach(var space in target)
				args.Add( space );
		} );
	}

	public Func<BuildEngine> GetBuildEngine = () => new BuildEngine(); // !!! instead of overriding this in Vengence, add event hook

	#endregion

	#region Configure Ravage

	public ConfigureRavage GetRavageConfiguration( Space space ) => _ravageConfig.ContainsKey( space ) ? _ravageConfig[space] : new ConfigureRavage();

	public void ModifyRavage( Space space, Action<ConfigureRavage> action ) {
		if(!_ravageConfig.ContainsKey( space ))
			_ravageConfig.Add( space, new ConfigureRavage() );
		action( _ravageConfig[space] );
	}

	readonly Dictionary<Space, ConfigureRavage> _ravageConfig = new Dictionary<Space, ConfigureRavage>(); // change ravage state of a Space

	#endregion

	public DahanGroupBinding DahanOn( Space space ) => Tokens[space].Dahan; // Obsolete - use TargetSpaceCtx

	#region API - overridable

	public Func<GameState,Space,AddBlightEffect>    AddBlightSideEffect = Default_AddBlightSideEffect; /// <summary> Hook so Stone's presence can stop the cascade / Destroy effects of blight. </summary>

	public Func<int, Space,Task> TakeFromBlightSouce {
		get { return _takeFromBlightSouce ?? Default_TakeBlightFromSource; }
		set { _takeFromBlightSouce = value; }
	}
	Func<int, Space, Task> _takeFromBlightSouce;

	public Func<Spirit,GameState,Cause,Task> Destroy1PresenceFromBlightCard = DefaultDestroy1PresenceFromBlightCard; // Direct distruction from Blight Card, not cascading

	void Heal( GameState obj ) => Healer.Heal( obj ); // called at end of round.
	public Healer Healer = new Healer(); // replacable Behavior

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
	static (Func<Space,bool>,string) InvaderCriteria(GameState gs) {
		bool NoCity(Space space) => gs.Tokens[space].Sum(Invader.City)==0;
		bool NoCityOrTown(Space space) => gs.Tokens[space].SumAny(Invader.City,Invader.Town)==0;
		bool NoInvader(Space space) => !gs.Tokens[space].HasInvaders();
		return gs.Fear.TerrorLevel switch {
			3 => (NoCity,"no cities"),
			2 => (NoCityOrTown,"no towns or cities"),
			_ => (NoInvader,"no invaders")
		};
	}

	// ! This is the only thing that needs checked after every action.
	void CheckTerrorLevelVictory(){
		var (filter,description) = InvaderCriteria(this);
		if( Island.AllSpaces.All( filter ) )
			GameOverException.Win($"Terror Level {Fear.TerrorLevel} - {description}");
	}

	#endregion

	#region Default API methods

	static private AddBlightEffect Default_AddBlightSideEffect( GameState gs, Space blightSpace ) {
		bool isFirstBlight = gs.Tokens[blightSpace].Blight.Count == 1;
		return new AddBlightEffect {
			DestroyPresence = true,
			Cascade = !isFirstBlight,
		};
	}

	/// <returns># of blight to remove from card</returns>
	Task Default_TakeBlightFromSource( int count, Space _ ) {
		if( count < 0 ) throw new ArgumentOutOfRangeException();
		this.blightOnCard -= count;
		return Task.CompletedTask;
	}

	static async Task DefaultDestroy1PresenceFromBlightCard( Spirit spirit, GameState gs, Cause cause ) {
		var presence = await spirit.Action.Decision( Select.DeployedPresence.ToDestroy( "Blighted Island: Select presence to destroy.", spirit ) );
		await spirit.Presence.Destroy( presence, gs, ActionType.BlightedIsland );
	}

	#endregion

	public void Log( ILogEntry entry ) => NewLogEntry?.Invoke( entry );

	public async Task TriggerTimePasses() {

		_ravageConfig.Clear();

		// Clear Defend
		foreach(var s in Island.AllSpaces)
			Tokens[s].Defend.Clear();

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
	public DualAsyncEvent<GameState> StartOfInvaderPhase   = new DualAsyncEvent<GameState>();         // Blight effects
	public DualAsyncEvent<RavagingEventArgs> PreRavaging   = new DualAsyncEvent<RavagingEventArgs>(); // A Spread of Rampant Green - Whole game - stop ravage
	public DualAsyncEvent<BuildingEventArgs> PreBuilding   = new DualAsyncEvent<BuildingEventArgs>(); // A Spread of Rampant Green - While game - stop build
	public DualAsyncEvent<ExploreEventArgs>  PreExplore    = new DualAsyncEvent<ExploreEventArgs>();
	public DualAsyncEvent<InvadersRavaged> InvadersRavaged = new DualAsyncEvent<InvadersRavaged>();
	public DualAsyncEvent<LandDamagedArgs> LandDamaged = new DualAsyncEvent<LandDamagedArgs>();         // Let Them Break Themselves Against the Stone

	public event Action<GameState> TimePasses_WholeGame;                                            // Spirit cleanup
	public Stack<Func<GameState, Task>> TimePasses_ThisRound = new Stack<Func<GameState, Task>>(); // This must be Push / Pop

	#endregion

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

	public virtual void Heal( GameState gs ) {
		foreach(var space in gs.Tokens.Keys.Except(skipHealSpaces))
			InvaderBinding.HealTokens( gs.Tokens[space] );
		skipHealSpaces.Clear();
	}

	public void Skip( Space space ) => skipHealSpaces.Add( space );

	protected List<Space> skipHealSpaces = new List<Space>();

}

public class LandDamagedArgs {
	public GameState GameState;
	public Space Space;
	public int Damage;
}

public class AddBlightEffect {
	public bool Cascade;
	public bool DestroyPresence;
}