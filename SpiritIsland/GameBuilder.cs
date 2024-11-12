
namespace SpiritIsland;

public class GameBuilder( params IGameComponentProvider[] _providers ) {

	public string[] SpiritNames => _providers.SelectMany(p => p.SpiritNames ).Order().ToArray();
	public string[] AdversaryNames => _providers.SelectMany(p => p.AdversaryNames).Order().ToArray();

	public Spirit[] BuildSpirits( params string[] spirits ) => spirits.Select(spirit 
		=> _providers.Select( p => p.MakeSpirit( spirit ) ).FirstOrDefault( x => x != null )
		?? throw new InvalidOperationException( $"Spirit named '{spirit}' not found." )
	).ToArray();

#pragma warning disable CA1822 // Mark members as static
	public Board[] BuildBoards( params string[] boardNames ) {
		BoardOrientation[] layout = boardNames.Length switch {
			1 => OneBoardLayout,
			2 => TwoBoardLayout,
			3 => ThreeBoardLayout,
			4 => FourBoardLayout,
			_ => throw new ArgumentOutOfRangeException( nameof( boardNames ), "Orienation of more than 4 boards not defined.")
		};
		Board[] boards = new Board[boardNames.Length];
		for(int i=0;i<boards.Length;++i)
			boards[i] = Board.BuildBoard( boardNames[i], layout[i] );
		return boards;
	}
	static public BoardOrientation[] OneBoardLayout => new[] { BoardOrientation.Home };
	static public BoardOrientation[] TwoBoardLayout => new[] { 
		BoardOrientation.Home,
		BoardOrientation.ToMatchSide( 0, BoardOrientation.Home.SideCoord(0))
	};
	static public BoardOrientation[] ThreeBoardLayout => new[] {
		BoardOrientation.Home,
		BoardOrientation.ToMatchSide( 0, BoardOrientation.Home.SideCoord(1)),
		BoardOrientation.ToMatchSide( 1, BoardOrientation.Home.SideCoord(0))
	};
	static public BoardOrientation[] FourBoardLayout { get {
		var bl = BoardOrientation.Home;
		BoardOrientation tl = BoardOrientation.ToMatchSide( 0, bl.SideCoord(2));
		BoardOrientation tr = BoardOrientation.ToMatchSide( 1, tl.SideCoord(1));
		BoardOrientation br = BoardOrientation.ToMatchSide( 1, bl.SideCoord(1));
		return [ bl,tl,tr,br ];
	}}
#pragma warning restore CA1822 // Mark members as static

	public IAdversary BuildAdversary( AdversaryConfig cfg ) {
		var adversary = _providers.Select( p => p.MakeAdversary( cfg?.Name ) ).FirstOrDefault( x => x != null )
			?? new NullAdversary();
		adversary.Level = cfg?.Level ?? 0;
		return adversary;
	}

	public PowerCard[] BuildMinorCards() => _providers.SelectMany( p => p.MinorCards ).ToArray();
	public PowerCard[] BuildMajorCards() => _providers.SelectMany( p => p.MajorCards ).ToArray();
	public List<IFearCard> BuildFearCards() => _providers.SelectMany( p => p.FearCards ).ToList();
	public List<BlightCard> BuildBlightCards() => _providers.SelectMany( p => p.BlightCards ).ToList();

	public int[] FearCardsPerLevel = [3, 3, 3];

	public GameState BuildGame( GameConfiguration cfg ) {
		Spirit[] spirits = BuildSpirits( cfg.Spirits );
		Board[] boards = BuildBoards( cfg.Boards );
		var blightCards = BuildBlightCards();

		// GameState
		var gameState = new GameState( spirits, boards, cfg.ShuffleNumber );

		// Game # - Random Seeds (don't change this order or this will change game definition)
		var randomizer = new Random( cfg.ShuffleNumber );
		int invaderSeed = randomizer.Next(); // 1
		int majorSeed = randomizer.Next();   // 2
		int minorSeed = randomizer.Next();   // 3
		int fearSeed = randomizer.Next();    // 4
		int blightSeed = randomizer.Next();  // 5

		// Adversary
		var adversary = BuildAdversary( cfg.Adversary );
		gameState.Adversary = adversary;

		// (1) Invader Deck
		gameState.InvaderDeck = adversary.InvaderDeckBuilder.Build( invaderSeed );

		// (2) Major Power Cards
		gameState.MajorCards = new PowerCardDeck( BuildMajorCards(), majorSeed, PowerType.Major );

		// (3) Minor Power Cards
		gameState.MinorCards = new PowerCardDeck( BuildMinorCards(), minorSeed, PowerType.Minor );

		// (4) Fear Cards
		adversary.AdjustFearCardCounts(FearCardsPerLevel); // !!! Use these to set GameState
		gameState.Fear.CardsPerLevel_Initial = FearCardsPerLevel;

		if(cfg.CommandTheBeasts)
			gameState.Fear.CardsPerLevel_Initial[1]++; // Command the Beasts

		// Fear Deck - ! this could be pushed into
		var fearCards = BuildFearCards();
		new Random( fearSeed ).Shuffle( fearCards );
		gameState.Fear.Deck.Clear();
		for(int i = 0; i < gameState.Fear.CardsPerLevel_Initial.Sum(); ++i)
			gameState.Fear.PushOntoDeck( fearCards[i] );

		// (5) Blight Cards
		new Random( blightSeed ).Shuffle( blightCards );
		gameState.BlightCards = blightCards;
		gameState.BlightCard = blightCards[0];
		blightCards.RemoveAt( 0 );

		adversary.Init( gameState );

		// Enable Win / Loss Check
		gameState.AddStandardWinLossChecks();

		gameState.Initialize();

		// After initializing: Starting-Tokens, Explore-Card, Blight Card (and spirits)
		adversary.AdjustPlacedTokens( gameState );

		if(cfg.CommandTheBeasts)
			CommandBeasts.Setup( gameState );

		return gameState;
	}

	class NullAdversary : IAdversary {
		public int Level { get => 0; set { } } // ignore
		public InvaderDeckBuilder InvaderDeckBuilder => InvaderDeckBuilder.Default;
		public int[] FearCardsPerLevel => [ 3, 3, 3 ];
		public void AdjustFearCardCounts(int[] counts) { }
		public AdversaryLevel[] Levels => [ new AdversaryLevel(0,0,3,3,3,"No Adversary","No Escalation") ];
		public IEnumerable<AdversaryLevel> ActiveLevels => [];
		public AdversaryLossCondition LossCondition => null;
		public string AdvName => null;
		public void AdjustPlacedTokens( GameState _ ) { }
		public void Init( GameState _ ) { }
		public string Describe() { return string.Empty;	}
	}

}
