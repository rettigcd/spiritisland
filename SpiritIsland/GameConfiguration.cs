namespace SpiritIsland;

public class GameConfiguration {

	public string Spirit;
	public string Board;
	public int ShuffleNumber;
	public AdversaryConfig Adversary;

	public string AdversarySummary => Adversary == null ? "[none]" : $"{Adversary.Name} {Adversary.Level}";

}

public class GameBuilder {

	readonly IGameComponentProvider[] _providers;

	public GameBuilder( params IGameComponentProvider[] providers ) {
		_providers = providers;
	}

	public string[] SpiritNames => _providers.SelectMany(p => p.SpiritNames ) .OrderBy( t => t ) .ToArray();
	public string[] AdversaryNames => _providers.SelectMany(p => p.AdversaryNames) .OrderBy( t => t ) .ToArray();

	public Spirit BuildSpirit( string spirit ) => _providers.Select( p => p.MakeSpirit( spirit ) ).FirstOrDefault( x => x != null )
		?? throw new InvalidOperationException( $"Spirit named '{spirit}' not found." );

#pragma warning disable CA1822 // Mark members as static
	public Board BuildBoard( string board ) => SpiritIsland.Board.BuildBoard( board );
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
	public List<IBlightCard> BuildBlightCards() => _providers.SelectMany( p => p.BlightCards ).ToList();

	public GameState BuildGame( GameConfiguration cfg ) {
		Spirit spirit = BuildSpirit( cfg.Spirit );
		Board board = BuildBoard( cfg.Board );
		var blightCards = BuildBlightCards();

		// GameState
		var gameState = new GameState( spirit, board );

		// Game # - Random Seeds (don't change this order or this will change game definition)
		var randomizer = new Random( cfg.ShuffleNumber );
		int invaderSeed = randomizer.Next(); // 1
		int majorSeed = randomizer.Next();   // 2
		int minorSeed = randomizer.Next();   // 3
		int fearSeed = randomizer.Next();    // 4
		int blightSeed = randomizer.Next();  // 5

		// Adversary
		var adversary = BuildAdversary( cfg.Adversary );

		// (1) Invader Deck
		gameState.InvaderDeck = new InvaderDeck( invaderSeed, adversary.InvaderCardOrder );

		// (2) Major Power Cards
		gameState.MajorCards = new PowerCardDeck( BuildMajorCards(), majorSeed );

		// (3) Minor Power Cards
		gameState.MinorCards = new PowerCardDeck( BuildMinorCards(), minorSeed );

		// (4) Fear Cards
		var fearCards = BuildFearCards();
		new Random( fearSeed ).Shuffle( fearCards );
		gameState.Fear.Deck.Clear();
		if(adversary.FearCardsPerLevel != null)
			gameState.Fear.cardsPerLevel = adversary.FearCardsPerLevel;
		for(int i = 0; i < gameState.Fear.cardsPerLevel.Sum(); ++i)
			gameState.Fear.PushOntoDeck( fearCards[i] );

		// (5) Blight Cards
		new Random( blightSeed ).Shuffle( blightCards );
		gameState.BlightCards = blightCards;
		gameState.BlightCard = blightCards[0];
		blightCards.RemoveAt( 0 );

		adversary.PreInitialization( gameState );

		// Enable Win / Loss Check
		gameState.ShouldCheckWinLoss = true; // !!! instead of this, load win/loss states into the check-list for real games

		gameState.Initialize();

		adversary.PostInitialization( gameState );

		Init_CommandTheBeasts( gameState );

		return gameState;
	}

	class NullAdversary : IAdversary {
		public int Level { set { } } // ignore
		public int[] InvaderCardOrder => new int[] { 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3 };
		public int[] FearCardsPerLevel => new int[] { 3, 3, 3 };

		public ScenarioLevel[] Adjustments => Array.Empty<ScenarioLevel>();

		public void PostInitialization( GameState _ ) { }
		public void PreInitialization( GameState _ ) { }
	}


	static void Init_CommandTheBeasts( GameState gameState ) {
		// If there are no Event cards, compensate with Command-the-Beasts

		var invaderCards = gameState.InvaderDeck.UnrevealedCards;
		void InitBeastCommand( int stage ) {
			for(int i = 0; i < invaderCards.Count; ++i) {
				if(invaderCards[i].InvaderStage != stage) continue;
				invaderCards[i] = new TriggerCommandBeasts( invaderCards[i] );
				break;
			}
		}
		InitBeastCommand( 2 );
		InitBeastCommand( 3 );
	}

}



public record AdversaryConfig( string Name, int Level );
