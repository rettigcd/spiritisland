namespace SpiritIsland;

public interface IAdversary {
	int Level { get; set; }
	InvaderDeckBuilder InvaderDeckBuilder { get; }
	int[] FearCardsPerLevel { get; }
	void AdjustFearCardCounts(int[] counts);

	/// <summary> Decks are already built, but tokens have not been placed yet. </summary>
	void Init( GameState gameState );
	/// <summary> Adjusts Tokens that are already placed. </summary>
	void AdjustPlacedTokens( GameState gamestate );

	AdversaryLevel[] Levels { get; }

	IEnumerable<AdversaryLevel> ActiveLevels { get; }

	AdversaryLossCondition LossCondition { get; }

	string AdvName { get; }
	public string Describe();
}
