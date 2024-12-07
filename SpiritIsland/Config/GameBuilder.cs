
namespace SpiritIsland;

public class GameBuilder( params IGameComponentProvider[] _providers ) {

	public string[] SpiritNames => _providers.SelectMany(p => p.SpiritNames ).Order().ToArray();

	public AspectConfigKey[] AspectNames => [.. _providers
		.SelectMany(p => p.AspectNames )
		.OrderBy(x=>x.Spirit).ThenBy(x=>x.Aspect)];

	public AspectConfigKey[] AspectsFor(string spiritName) => [.. _providers
		.SelectMany(p => p.AspectNames)
		.Where(x=>x.Spirit==spiritName)
		.OrderBy(x => x.Aspect)];

	public string[] AdversaryNames => _providers.SelectMany(p => p.AdversaryNames).Order().ToArray();

	public Spirit[] BuildSpirits( string[] spirits, AspectConfigKey[] aspectKeys = null) {
		aspectKeys ??= [];
		return spirits.Select(s => Build1Spirit(s, aspectKeys)).ToArray();
	}

	Spirit Build1Spirit(string spiritName, AspectConfigKey[] aspectKeys) {
		var spirit = _providers.Select(p => p.MakeSpirit(spiritName)).FirstOrDefault(x => x != null) 
			?? throw new InvalidOperationException($"Spirit named '{spiritName}' not found.");
		foreach(var aspectKey in aspectKeys.Where(k=>k.Spirit == spiritName))
			Build1Aspect(aspectKey).ModSpirit(spirit);
		return spirit;
	}

	//IAspect Build1Aspect(AspectConfigKey key) => _providers.Select(p => p.MakeAspect(key)).FirstOrDefault()
	//	?? throw new ArgumentException($"Unable to build Aspect found for [{key.Spirit}-{key.Aspect}]");

	IAspect Build1Aspect(AspectConfigKey key) { 
		foreach(var provider in _providers ) {
			var aspect = provider.MakeAspect(key);
			if(aspect is not null) return aspect;
		}
		throw new ArgumentException($"Unable to build Aspect found for [{key.Spirit}-{key.Aspect}]");
	}

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
			boards[i] = BoardFactory.Build( boardNames[i], layout[i] );
		return boards;
	}
	static public BoardOrientation[] OneBoardLayout => [BoardOrientation.Home];
	static public BoardOrientation[] TwoBoardLayout => [ 
		BoardOrientation.Home,
		BoardOrientation.ToMatchSide( 0, BoardOrientation.Home.SideCoord(0))
	];
	static public BoardOrientation[] ThreeBoardLayout => [
		BoardOrientation.Home,
		BoardOrientation.ToMatchSide( 0, BoardOrientation.Home.SideCoord(1)),
		BoardOrientation.ToMatchSide( 1, BoardOrientation.Home.SideCoord(0))
	];
	static public BoardOrientation[] FourBoardLayout { get {
		var bl = BoardOrientation.Home;
		BoardOrientation tl = BoardOrientation.ToMatchSide( 0, bl.SideCoord(2));
		BoardOrientation tr = BoardOrientation.ToMatchSide( 1, tl.SideCoord(1));
		BoardOrientation br = BoardOrientation.ToMatchSide( 1, bl.SideCoord(1));
		return [ bl,tl,tr,br ];
	}}
#pragma warning restore CA1822 // Mark members as static

	public IAdversary BuildAdversary( AdversaryConfig cfg ) {
		return cfg is null ? new NullAdversaryBuilder().Build(0)
			: GetAdversaryBuilder(cfg.Name).Build(cfg.Level);
	}

	public IAdversaryBuilder GetAdversaryBuilder(string adversaryName) {
		return _providers
			.Select(p => p.MakeAdversary(adversaryName))
			.FirstOrDefault(x => x != null)
			?? new NullAdversaryBuilder();
	}

	public PowerCard[] BuildMinorCards() => _providers.SelectMany( p => p.MinorCards ).ToArray();
	public PowerCard[] BuildMajorCards() => _providers.SelectMany( p => p.MajorCards ).ToArray();
	public List<IFearCard> BuildFearCards() => _providers.SelectMany( p => p.FearCards ).ToList();
	public List<BlightCard> BuildBlightCards() => _providers.SelectMany( p => p.BlightCards ).ToList();

	public GameState BuildShell( GameConfiguration cfg) {
		Spirit[] spirits = BuildSpirits(cfg.Spirits, cfg.Aspects);
		Board[] boards = BuildBoards(cfg.Boards);
		return new GameState(spirits, boards, cfg.ShuffleNumber);
	}

	public GameState BuildGame( GameConfiguration cfg ) {
		var blightCards = BuildBlightCards();

		// GameState
		var gameState = BuildShell(cfg);

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
		int[] fearCardsPerLevel = [3, 3, 3]; 
		adversary.AdjustFearCardCounts(fearCardsPerLevel);
		gameState.Fear.CardsPerLevel_Initial = fearCardsPerLevel;

		// Fear Deck - ! this could be pushed into
		var fearCards = BuildFearCards();
		new Random( fearSeed ).Shuffle( fearCards );
		int neededFearCards = fearCardsPerLevel.Sum();
		gameState.Fear.Deck.Clear();
		for(int i = 0; i < neededFearCards; ++i)
			gameState.Fear.PushOntoDeck( fearCards[i] );

		if( cfg.CommandTheBeasts )
			gameState.Fear.CardsPerLevel_Initial[1]++; // Command the Beasts

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

	class NullAdversaryBuilder : IAdversaryBuilder {
		public string Name => "No Adversary";
		static public AdversaryLevel Level => new AdversaryLevel(_level: 0, _difficulty: 0, _fear1: 3, _fear2: 3, _fear3: 3, string.Empty);
		public AdversaryLevel[] Levels => [Level];
		public AdversaryLossCondition LossCondition => null;
		public IAdversary Build(int _) => new Adversary(this, 0);
	}

}
