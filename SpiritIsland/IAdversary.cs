namespace SpiritIsland;

public interface IAdversary {
	int Level { set; }
	void Adjust(GameState gameState);
	int[] InvaderCardOrder { get; }
	int[] FearCardsPerLevel { get; }
	void AdjustInvaderDeck( InvaderDeck deck );
}
