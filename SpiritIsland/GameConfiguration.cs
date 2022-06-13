namespace SpiritIsland;

public class GameConfiguration {

	public int ShuffleNumber;
	public Type SpiritType;
	public string Board;
	public string Color;
	public Type Adversary;
	public int AdversaryLevel;

	public GameState BuildGame( IGameComponentProvider[] providers ) {

		Spirit spirit = (Spirit)Activator.CreateInstance( SpiritType );

		Board board = Board switch {
			"A" => SpiritIsland.Board.BuildBoardA(),
			"B" => SpiritIsland.Board.BuildBoardB(),
			"C" => SpiritIsland.Board.BuildBoardC(),
			"D" => SpiritIsland.Board.BuildBoardD(),
			_ => null,
		};

		var majorCards = new List<PowerCard>();
		var minorCards = new List<PowerCard>();
		var fearCards = new List<IFearOptions>();
		var blightCards = new List<IBlightCard>();

		foreach(var provider in providers) {
			minorCards.AddRange( provider.MinorCards );
			majorCards.AddRange( provider.MajorCards );
			fearCards.AddRange( provider.FearCards );
			blightCards.AddRange( provider.BlightCards );
		}

		// GameState
		var gameState = new GameState( spirit, board );
		int[] invaderCardOrder = new int[] { 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3 };
		int[] fearCardsPerLevel = new int[] { 3, 3, 3 };
		Action<InvaderDeck> adjustInvaderDeck = ( deck ) => { };

		// Game # - Random Seeds (don't change this order or this will change game definition)
		var randomizer = new Random( ShuffleNumber );
		int invaderSeed = randomizer.Next(); // 1
		int majorSeed = randomizer.Next();   // 2
		int minorSeed = randomizer.Next();   // 3
		int fearSeed = randomizer.Next();    // 4
		int blightSeed = randomizer.Next();  // 5

		if(Adversary != null) {
			var adversary = (IAdversary)Activator.CreateInstance( Adversary );
			adversary.Level = AdversaryLevel;
			adversary.Adjust( gameState );
			invaderCardOrder = adversary.InvaderCardOrder ?? invaderCardOrder;
			fearCardsPerLevel = adversary.FearCardsPerLevel ?? fearCardsPerLevel;
			adjustInvaderDeck = adversary.AdjustInvaderDeck;
		}

		// (1) Invader Deck
		gameState.InvaderDeck = new InvaderDeck( invaderSeed, invaderCardOrder );
		adjustInvaderDeck( gameState.InvaderDeck );
			
		// (2) Major Power Cards
		gameState.MajorCards = new PowerCardDeck( majorCards.ToArray(), majorSeed );

		// (3) Minor Power Cards
		gameState.MinorCards = new PowerCardDeck( minorCards.ToArray(), minorSeed );

		// (4) Fear Cards
		new Random(fearSeed).Shuffle( fearCards );
		gameState.Fear.Deck.Clear();
		gameState.Fear.cardsPerLevel = fearCardsPerLevel;
		for(int i=0;i<gameState.Fear.cardsPerLevel.Sum();++i)
			gameState.Fear.AddCard( fearCards[i] );

		// (5) Blight Cards
		new Random(blightSeed).Shuffle( blightCards );
		gameState.BlightCards = blightCards;
		gameState.BlightCard = blightCards[0];
		blightCards.RemoveAt(0);

		// No Events
		var invaderCards = gameState.InvaderDeck.UnrevealedCards;
		void InitBeastCommand(int stage) {
			for(int i = 0; i < invaderCards.Count; ++i) {
				if(invaderCards[i].InvaderStage != stage) continue;
				invaderCards[i] = new TriggerCommandBeasts( invaderCards[i] );
				break;
			}
		}
		InitBeastCommand(2);
		InitBeastCommand(3);

		// Enable Win / Loss Check
		gameState.ShouldCheckWinLoss = true; // !!! instead of this, load win/loss states into the check-list for real games

		return gameState;
	}

}