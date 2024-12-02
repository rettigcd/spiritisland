using SpiritIsland.Log;

namespace SpiritIsland;

public class GameState : IHaveMemento {

	public static GameState Current => ActionScope.Current?.GameState;

	/// <summary>
	/// Caches the root ActionScope created when GameState constructed,
	/// Allowing UI thread to access it also.
	/// </summary>
	public readonly ActionScope RootScope;

	#region constructors

	public GameState(Spirit[] spirits,Board[] boards, int gameNumber = 0 ){
		if(spirits.Length==0) throw new ArgumentException("Game must include at least 1 spirit");

		RootScope = new ActionScope(this);
		ActionScope.Initialize(RootScope); // This must be called on all Execution Contexts 

		Island = new Island( boards );
		Spirits = spirits;
		ShuffleNumber = gameNumber;

		// Note: don't init invader deck here, let users substitute
		RoundNumber = 1;
		Fear = new Fear( this );
		Tokens = new Tokens_ForIsland();
		BlightCard = new NullBlightCard(); // do AFTER Tokens.  auto-binds to the tokens to hold blight

		AddTimePassesAction( Tokens );
		AddTimePassesAction( Healer );


	}
	public int ShuffleNumber { get; } // used to generate different shuffle #s

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
		InvaderDeck.InitExploreSlotAsync().Wait();
		InvaderDeck.Explore.Execute( this ).Wait();
		InvaderDeck.AdvanceAsync().Wait();
	}

	void PlaceStartingTokens() {
		foreach(var board in Island.Boards) {
			Tokens[board[2]].Disease.Init( 1 );
			var lowest = board.Spaces.Skip( 1 ).OfType<SingleSpaceSpec>().First( s => s.StartUpCounts.IsEmpty );
			Tokens[lowest].Beasts.Adjust( 1 );
		}

		foreach(var board in Island.Boards)
			foreach(var space in board.Spaces)
				((SingleSpaceSpec)space).InitTokens( Tokens[space] );
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

	public IAdversary Adversary { get; set; }

	// == Components ==
	public Fear Fear { get; }
	public Island Island { get; set; }
	public Spirit[] Spirits { get; }

	public Tokens_ForIsland Tokens { get; }

	public List<SpaceSpec> OtherSpaces = []; // Currently only used for EndlessDarkness
	public List<object> ReminderCards = []; // !!! Save to Memento

	public PowerCardDeck MajorCards {get; set; }
	public PowerCardDeck MinorCards { 
		get; 
		set;
	}
	public readonly Healer Healer = new Healer();

	public List<BlightCard> BlightCards = []; // Deck of Blight Cards
	public BlightCard BlightCard { 
		get => _blightCard; 
		set { 
			_blightCard = value;
			_blightCard.Bind(this);
		}
	}
	BlightCard _blightCard;

	public GameOverLogEntry Result = null;

	public event Action<ILogEntry> NewLogEntry; // API

	public IEnumerable<Space> Spaces => SpaceSpecs.Select(s => Tokens[s]);
	public IEnumerable<Space> Spaces_Existing => SpaceSpecs_Existing.Select(s => Tokens[s]);
	public IEnumerable<Space> Spaces_Unfiltered => SpaceSpecs_Unfiltered.Select(s => Tokens[s]);

	/// <summary> Non-stasis + InPlay </summary>
	public IEnumerable<SpaceSpec> SpaceSpecs => Island.Boards.SelectMany(b => b.Spaces).Distinct();
	/// <summary> All Non-stasis (even not-in-play) </summary>
	public IEnumerable<SpaceSpec> SpaceSpecs_Existing => Island.Boards.SelectMany(b => b.Spaces_Existing).Distinct();
	/// <summary> Includes "Other" spaces like EndlessDark. </summary>
	public IEnumerable<SpaceSpec> SpaceSpecs_Unfiltered => Island.Boards.SelectMany(b => b.Spaces_Unfiltered).Distinct().Union(OtherSpaces);

	public InvaderDeck InvaderDeck { 
		get { return _invaderDeck ??= InvaderDeckBuilder.Default.Build(); }
		set { _invaderDeck = value; }
	}
	InvaderDeck _invaderDeck;

	public void AddIslandMod( BaseModEntity mod ) => Tokens.AddIslandMod( mod );

	public void AddToAllActiveSpaces( BaseModEntity mod ) {
		foreach(var space in Spaces_Existing)
			space.Adjust(mod,1);
	}

	#region Win / Loss

	readonly List<Action<GameState>> WinLossChecks = [];

	/// <summary>
	/// Used to add the standard TerrorLevelVictory check or custom Adversary checks
	/// </summary>
	public void AddStandardWinLossChecks() {
		WinLossChecks.Add( CheckTerrorLevelVictory );
		WinLossChecks.Add( CheckIfSpiritIsDestroyed );
		WinLossChecks.Add( CheckIfTimeRunsOut );
	}

	public void AddWinLossCheck( Action<GameState> action ) => WinLossChecks.Add( action );

	/// <summary> Throws GameOverException when Win or Loss condition is met. </summary>
	public void CheckWinLoss() {
		foreach(Action<GameState> check in WinLossChecks)
			check(this);
	}

	// Win Loss Predicates
	static void CheckTerrorLevelVictory( GameState gs ){

		bool NoCity( Space space ) => space.Sum( Human.City ) == 0;
		bool NoCityOrTown( Space space ) => space.SumAny( Human.Town_City ) == 0;
		bool NoInvader( Space space ) => !space.HasInvaders();
		var (filter,description) = gs.Fear.TerrorLevel switch {
			4 => (_ => true, "Victory"),
			3 => ((Func<Space,bool>)NoCity, "no cities"),
			2 => (NoCityOrTown, "no towns or cities"),
			_ => (NoInvader, "no invaders")
		};
		if(ActionScope.Current.Spaces_Unfiltered.All( filter ) )
			GameOverException.Win($"Terror Level {gs.Fear.TerrorLevel} - {description}");
	}

	static void CheckIfSpiritIsDestroyed( GameState gs ) {
		foreach(Spirit spirit in gs.Spirits)
			if(!spirit.Presence.IsOnIsland)
				GameOverException.Lost( $"{spirit.SpiritName} is Destroyed" );
	}

	static void CheckIfTimeRunsOut( GameState gs ) {
		var deck = gs.InvaderDeck;
		if(deck.Explore.Cards.Count == 0 && deck.UnrevealedCards.Count == 0)
			GameOverException.Lost( "Time runs out" );
	}

	#endregion

	public void Log( ILogEntry entry ) => NewLogEntry?.Invoke( entry );
	public async Task TriggerTimePasses() {

		// Clear Defend
		foreach(var s in ActionScope.Current.Spaces_Unfiltered )
			s.Defend.Clear();

		await RunTimePassesActions();

		++RoundNumber;
	}

	// - Events -


	#region Configuration - TERRAIN


	// !! If we decide to split up Config stuff, move this to ActionScope
	// because ActionCategory is the Key and this has nothing to do with GameState other than it holds Config info
	readonly TerrainMapper DefaultTerrain = new TerrainMapper(); // Default
	readonly Dictionary<ActionCategory,TerrainMapper> _terrains = [];
	public TerrainMapper Terrain_ForBlight = new TerrainMapper(); // This is ONLY called for blight inside gamestate.

	public TerrainMapper GetTerrain( ActionCategory cat ) => _terrains.TryGetValue( cat, out TerrainMapper value ) ? value : DefaultTerrain;

	public void ReplaceTerrain(Func<TerrainMapper,TerrainMapper> replacer, params ActionCategory[] cats ) {
		foreach(var cat in cats)
			_terrains[cat] = replacer( GetTerrain(cat) );
	}

	#endregion Configuration - TERRAIN

	#region Memento

	object IHaveMemento.Memento {
		get => new MyMemento( this );
		set => ((MyMemento)value).Restore( this );
	}

	class MyMemento {
		public MyMemento(GameState src) {
			_mementos.SaveMany( src.Spirits );
			_mementos.Save( src.MajorCards );
			_mementos.Save( src.MinorCards );
			_mementos.Save( src.InvaderDeck );
			_mementos.Save( src.Fear );
			_mementos.Save( src.BlightCard );
			_mementos.Save( src.Island );
			_mementos.Save( src.Tokens );
			_mementos.Save( src._timePassesActions );
			_mementos.Save( src._postInvaderPhaseActions );
			_mementos.Save( src._preInvaderPhaseActions);

			_roundNumber        = src.RoundNumber;
			_isBlighted         = src.BlightCard.CardFlipped;
		}

		public void Restore(GameState src ) {
			_mementos.Restore();

			src.RoundNumber = _roundNumber;
			src.BlightCard.CardFlipped = _isBlighted;
		}

		readonly int _roundNumber;
		readonly bool _isBlighted;
		readonly Dictionary<IHaveMemento,object> _mementos = [];
	}

	#endregion Memento

	#region Hooks

    public Task RunPreInvaderActions() => _preInvaderPhaseActions.Run(this);
	public void AddPreInvaderPhaseAction(IRunBeforeInvaderPhase action) => _preInvaderPhaseActions.Add(action);
    readonly PreInvaderPhaseActionList _preInvaderPhaseActions = new();

    public Task RunPostInvaderActions() => _postInvaderPhaseActions.Run(this);
	public void AddPostInvaderPhase(IRunAfterInvaderPhase action) => _postInvaderPhaseActions.Add(action);
    readonly PostInvaderPhaseActionList _postInvaderPhaseActions = new();


    public Task RunTimePassesActions() => _timePassesActions.Run(this);
    public void AddTimePassesAction( IRunWhenTimePasses action ) => _timePassesActions.Add(action);
	readonly TimePassesActionList _timePassesActions = new();

    #endregion

}
