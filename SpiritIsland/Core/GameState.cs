using SpiritIsland.Log;

namespace SpiritIsland;

public class GameState : IHaveHealthPenaltyPerStrife {

	public static GameState Current => _current.Value; // !! We might want to only access from the ActionScope
	readonly static AsyncLocal<GameState> _current = new AsyncLocal<GameState>(); // value gets shallow-copied into child calls and post-awaited states.

	#region constructors

	/// <summary>
	/// Simplified constructor for single-player
	/// </summary>
	public GameState( Spirit spirit, Board board, int gameNumber = 0 ) 
		: this(new Spirit[]{ spirit }, new Board[] {board}, gameNumber ) {}

	public GameState(Spirit[] spirits,Board[] boards, int gameNumber = 0 ){
		if(spirits.Length==0) throw new ArgumentException("Game must include at least 1 spirit");

		_current.Value = this;

		Island = new Island( boards );
		Spirits = spirits;
		ShuffleNumber = gameNumber;

		// Note: don't init invader deck here, let users substitute
		RoundNumber = 1;
		Fear = new Fear( this );
		Tokens = new Tokens_ForIsland();

		AddTimePassesAction( Tokens );
		AddTimePassesAction( Healer ); // !!! Shroud needs to be able to replace this.

		ActionScope.Initialize(); // ! This is here for tests.
	}
	public int ShuffleNumber { get; } // used to generate different shuffle #s

	/// <summary>
	/// Called AFTER everything has been configured. and BEFORE players make first move.
	/// </summary>
	public void Initialize() {
		ActionScope.Initialize();
		PlaceStartingTokens(); 
		InitialExplore();
		InitSpirits();// ManyMinds requires the beast to be in place, so this goes after tokens are placed.
		BlightCard.OnGameStart( this );
	}

	void InitialExplore() {
		InvaderDeck.InitExploreSlot();
		InvaderDeck.Explore.Execute( this ).Wait();
		InvaderDeck.Advance();
	}

	void PlaceStartingTokens() {
		foreach(var board in Island.Boards) {
			Tokens[board[2]].Disease.Init( 1 );
			var lowest = board.Spaces.Skip( 1 ).OfType<Space1>().First( s => s.StartUpCounts.IsEmpty );
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

	/// <summary> Non-stasis + InPlay </summary>
	public IEnumerable<SpaceState> Spaces            => Island.Boards
		.SelectMany( b => b.Spaces )
		.Distinct() // MultiSpaces
		.Tokens();

	/// <summary> All Non-stasis (even not-in-play) </summary>
	public IEnumerable<SpaceState> Spaces_Existing   => Island.Boards
		.SelectMany( b => b.Spaces_Existing )
		.Distinct() // MultiSpaces
		.Tokens();

	public IEnumerable<SpaceState> Spaces_Unfiltered => Island.Boards
		.SelectMany( b => b.Spaces_Unfiltered )
		.Distinct() // MultiSpaces
		.Union(OtherSpaces)
		.Tokens();

	public List<Space> OtherSpaces = new List<Space>(); // Currently only used for EndlessDarkness

	public PowerCardDeck MajorCards {get; set; }
	public PowerCardDeck MinorCards { get; set; }
	public InvaderDeck InvaderDeck { 
		get { return _invaderDeck ??= InvaderDeckBuilder.Default.Build(); }
		set { _invaderDeck = value; }
	}
	InvaderDeck _invaderDeck;

	public void blightOnCard_Add( int count ) 
		=> Tokens[SpiritIsland.BlightCard.Space].Adjust( Token.Blight, count );

	public IBlightCard BlightCard = new NullBlightCard();
	public List<IBlightCard> BlightCards = new List<IBlightCard>();
	public GameOver Result = null;
	public int HealthPenaltyPerStrife { get; set; } = 0;

	#region Blight

	public int DamageToBlightLand = 2;

	public IEnumerable<SpaceState> CascadingBlightOptions( SpaceState ss ) => ss.Adjacent_Existing
		 .Where( x => !Terrain_ForBlight.MatchesTerrain( x, Terrain.Ocean ) // normal case,
			 || Terrain_ForBlight.MatchesTerrain( x, Terrain.Wetland ) );

	#endregion

	public void AddIslandMod( BaseModEntity mod ) => Tokens.AddIslandMod( mod );

	public void AddToAllActiveSpaces( BaseModEntity mod ) {
		foreach(var space in Spaces_Existing)
			space.Adjust(mod,1);
	}

	#region Win / Loss

	readonly List<Action<GameState>> WinLossChecks = new List<Action<GameState>>();

	/// <summary>
	/// Used to add the standard TerrorLevelVictory check or custom Adversary checks
	/// </summary>
	public void AddStandardWinLossChecks() {
		WinLossChecks.Add( CheckTerrorLevelVictory );
		WinLossChecks.Add( CheckIfSpiritIsDestroyed );
		WinLossChecks.Add( CheckIfTimeRunsOut );
	}

	public void AddWinLossCheck( Action<GameState> action ) => WinLossChecks.Add( action );

	public void CheckWinLoss() {
		foreach(Action<GameState> check in WinLossChecks)
			check(this);
	}

	// Win Loss Predicates
	static void CheckTerrorLevelVictory( GameState gs ){

		bool NoCity( SpaceState space ) => space.Sum( Human.City ) == 0;
		bool NoCityOrTown( SpaceState space ) => space.SumAny( Human.Town_City ) == 0;
		bool NoInvader( SpaceState space ) => !space.HasInvaders();
		var (filter,description) = gs.Fear.TerrorLevel switch {
			4 => (_ => true, "Victory"),
			3 => ((Func<SpaceState,bool>)NoCity, "no cities"),
			2 => (NoCityOrTown, "no towns or cities"),
			_ => (NoInvader, "no invaders")
		};
		if( gs.Spaces_Unfiltered.All( filter ) )
			GameOverException.Win($"Terror Level {gs.Fear.TerrorLevel} - {description}");
	}

	static void CheckIfSpiritIsDestroyed( GameState gs ) {
		foreach(Spirit spirit in gs.Spirits)
			if(!spirit.Presence.IsOnIsland)
				GameOverException.Lost( $"{spirit.Text} is Destroyed" );
	}

	static void CheckIfTimeRunsOut( GameState gs ) {
		var deck = gs.InvaderDeck;
		if(deck.Explore.Cards.Count == 0 && deck.UnrevealedCards.Count == 0)
			GameOverException.Lost( "Time runs out" );
	}

	#endregion

	#region Default API methods

	/// <returns># of blight to remove from card</returns>
	public async Task TakeBlightFromCard( int count ) {
		if( count < 0 ) throw new ArgumentOutOfRangeException(nameof(count));
		var blightCard = Tokens[SpiritIsland.BlightCard.Space];

		await blightCard.RemoveAsync(Token.Blight, count, RemoveReason.TakingFromCard ); // stops from putting back on card

		if(BlightCard != null && blightCard[Token.Blight] <= 0) {
			await Spirits[0].Select( "Island blighted", new IOption[] { BlightCard }, Present.Always );
			Log( new IslandBlighted( BlightCard ) );
			await BlightCard.OnBlightDepleated( this );
		}
	}

	#endregion

	public void Log( ILogEntry entry ) => NewLogEntry?.Invoke( entry );
	public void LogDebug( string debugMsg ) => Log( new Debug(debugMsg) );
	public async Task TriggerTimePasses() {

		// Clear Defend
		foreach(var s in Spaces_Unfiltered)
			s.Defend.Clear();

		// Do Custom end-of-round cleanup stuff before round switches over
		// (shifting memory need cards it is going to forget to still be in hand when calling .Forget() on it)
		for(int i=0; i<_timePassesActions.Count;++i){
			IRunWhenTimePasses action = _timePassesActions[i];
			await action.TimePasses(this);
			if( action.RemoveAfterRun )
				_timePassesActions.RemoveAt(i--);
		}

		// Do the standard round-switch-over stuff.
		if(TimePasses_WholeGame != null)
			await TimePasses_WholeGame.Invoke( this ); // can't use await and ?.Invoke together, => tries to await a null
		++RoundNumber;
	}

	#region Events

	// - Events -
	public event Action<ILogEntry> NewLogEntry;
	public AsyncEvent<GameState> StartOfInvaderPhase = new(); // Blight effects

	public event Func<GameState,Task> TimePasses_WholeGame;
	readonly List<IRunWhenTimePasses> _timePassesActions = new List<IRunWhenTimePasses>();

	public void AddTimePassesAction(IRunWhenTimePasses action ) {
		_timePassesActions.Add(action);
	}

	#endregion

	#region Configuration - Game-Wide overrideable / Behavior

	public readonly RavageBehavior DefaultRavageBehavior = new RavageBehavior();

	public Healer Healer = new Healer(); // replacable Behavior

	// !! If we decide to split up Config stuff, move this to ActionScope
	// because ActionCategory is the Key and this has nothing to do with GameState other than it holds Config info
	readonly TerrainMapper DefaultTerrain = new TerrainMapper(); // Default
	readonly Dictionary<ActionCategory,TerrainMapper> _terrains = new Dictionary<ActionCategory, TerrainMapper>();
	public TerrainMapper Terrain_ForBlight = new TerrainMapper(); // This is ONLY called for blight inside gamestate.

	public TerrainMapper GetTerrain( ActionCategory cat ) => _terrains.ContainsKey(cat) ? _terrains[cat] : DefaultTerrain;

	public void ReplaceTerrain(Func<TerrainMapper,TerrainMapper> replacer, params ActionCategory[] cats ) {
		foreach(var cat in cats)
			_terrains[cat] = replacer( GetTerrain(cat) );
	}

	// User Preference Stuff
	// CastDown uses Ocean vs Destroyed
	// Use Pre-select/pre-load

	#endregion overrideable Game-Wide Behavior

	#region Memento

	public virtual IMemento<GameState> SaveToMemento() => new Memento(this);
	public virtual void LoadFrom( IMemento<GameState> memento ) => ((Memento)memento).Restore(this);

	protected class Memento : IMemento<GameState> {
		public Memento(GameState src) {
			roundNumber  = src.RoundNumber;
			isBlighted   = src.BlightCard.CardFlipped;
			spirits      = src.Spirits.Select(s=>s.SaveToMemento()).ToArray();
			if(src.MajorCards != null) major = src.MajorCards.SaveToMemento();
			if(src.MinorCards != null) minor = src.MinorCards.SaveToMemento();
			invaderDeck  = src.InvaderDeck.SaveToMemento();
			fear         = src.Fear.SaveToMemento();
			tokens       = src.Tokens.SaveToMemento();
			startOfInvaderPhase = src.StartOfInvaderPhase.SaveToMemento();
			island = src.Island.SaveToMemento();
			damageToBlightLand = src.DamageToBlightLand;
		}
		public void Restore(GameState src ) {
			src.RoundNumber = roundNumber;
			src.BlightCard.CardFlipped = isBlighted;
			for(int i=0;i<spirits.Length;++i) src.Spirits[i].LoadFrom( spirits[i] );
			src.MajorCards?.RestoreFrom( major );
			src.MinorCards?.RestoreFrom( minor );
			src.InvaderDeck.LoadFrom( invaderDeck );
			src.Fear.LoadFrom( fear );
			src.Tokens.LoadFrom( tokens );
			src.StartOfInvaderPhase.LoadFrom( startOfInvaderPhase );
			src.Island.LoadFrom( island );
			src.DamageToBlightLand = damageToBlightLand;
		}
		readonly int roundNumber;
		readonly bool isBlighted;
		readonly IMemento<Spirit>[] spirits;
		readonly IMemento<PowerCardDeck> major;
		readonly IMemento<PowerCardDeck> minor;
		readonly IMemento<InvaderDeck> invaderDeck;
		readonly IMemento<Fear> fear;
		readonly IMemento<Tokens_ForIsland> tokens;
		readonly IMemento<AsyncEvent<GameState>> startOfInvaderPhase;
		readonly IMemento<Island> island;
		readonly int damageToBlightLand;
	}

	#endregion Memento

}

public enum RunCount { Once, All }
public interface IRunWhenTimePasses : ISpaceEntity {
	/// <returns>If object should be Removed from the TimePasses</returns>
	Task TimePasses( GameState gameState );
	/// <summary> Indicates if action should be removed after running once. </summary>
	bool RemoveAfterRun { get; }
}

public class TimePassesAction : IRunWhenTimePasses {

	public TimePassesAction(Func<GameState,Task> func, RunCount keepOrRemove ) {
		_func = func;
		_runCount = keepOrRemove;
	}
	public TimePassesAction( Action<GameState> action, RunCount keepOrRemove ) {
		_func = (gs) => { action(gs); return Task.CompletedTask; };
		_runCount = keepOrRemove;
	}

	bool IRunWhenTimePasses.RemoveAfterRun => _runCount == RunCount.Once;

	async Task IRunWhenTimePasses.TimePasses( GameState gameState ){
		await _func(gameState);
	}
	readonly Func<GameState,Task> _func;
	readonly RunCount _runCount;
}

public class Healer : IRunWhenTimePasses {

	bool IRunWhenTimePasses.RemoveAfterRun => false;
	public virtual Task TimePasses( GameState gameState ) {
		foreach(SpaceState ss in gameState.Spaces_Unfiltered)
			HealSpace( ss );
		skipHealSpaces.Clear();
		return Task.CompletedTask;
	}

	public virtual void HealSpace( SpaceState tokens ) {
		if( !skipHealSpaces.Contains(tokens.Space) )
			InvaderBinding.HealTokens( tokens );
	}

	public void Skip( Space space ) => skipHealSpaces.Add( space );

	protected HashSet<Space> skipHealSpaces = new HashSet<Space>();

}

public class LandDamagedArgs { public SpaceState Space; }
