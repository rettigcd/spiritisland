namespace SpiritIsland;

public class GameConfiguration {

	public string Spirit;
	public string Board;
	public int ShuffleNumber;
	public AdversaryConfig Adversary;

	public string AdversarySummary => Adversary == null ? "[none]" : $"{Adversary.Name} {Adversary.Level}";

	public GameState BuildGame( IGameComponentProvider[] providers, Func<string, IAdversary> buildAdversary ) {

		Spirit spirit = providers.Select( p => p.MakeSpirit( Spirit ) ).FirstOrDefault(x=>x!=null)
			?? throw new InvalidOperationException($"Spirit named '{Spirit}' not found.");

		Board board = SpiritIsland.Board.BuildBoard(Board);

		var majorCards = new List<PowerCard>();
		var minorCards = new List<PowerCard>();
		var fearCards = new List<IFearCard>();
		var blightCards = new List<IBlightCard>();

		foreach(var provider in providers) {
			minorCards.AddRange( provider.MinorCards );
			majorCards.AddRange( provider.MajorCards );
			fearCards.AddRange( provider.FearCards );
			blightCards.AddRange( provider.BlightCards );
		}

		// GameState
		var gameState = new GameState( spirit, board );

		// Game # - Random Seeds (don't change this order or this will change game definition)
		var randomizer = new Random( ShuffleNumber );
		int invaderSeed = randomizer.Next(); // 1
		int majorSeed = randomizer.Next();   // 2
		int minorSeed = randomizer.Next();   // 3
		int fearSeed = randomizer.Next();    // 4
		int blightSeed = randomizer.Next();  // 5

		// Adversary
		var adversary = buildAdversary( Adversary?.Name ) ?? new NullAdversary();
		// (1) Invader Deck
		gameState.InvaderDeck = new InvaderDeck( invaderSeed, adversary.InvaderCardOrder );

		// (2) Major Power Cards
		gameState.MajorCards = new PowerCardDeck( majorCards.ToArray(), majorSeed );

		// (3) Minor Power Cards
		gameState.MinorCards = new PowerCardDeck( minorCards.ToArray(), minorSeed );

		// (4) Fear Cards
		new Random( fearSeed ).Shuffle( fearCards );
		gameState.Fear.Deck.Clear();
		if( adversary.FearCardsPerLevel != null)
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
