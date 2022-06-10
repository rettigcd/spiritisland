namespace SpiritIsland;

public interface IAdversary {
	void Adjust(GameState board);
	int[] InvaderCardOrder { get; }
	int[] FearCardsPerLevel { get; }
}
