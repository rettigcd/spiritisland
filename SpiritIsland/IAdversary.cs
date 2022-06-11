namespace SpiritIsland;

public interface IAdversary {
	int Level { set; }
	void Adjust(GameState board);
	int[] InvaderCardOrder { get; }
	int[] FearCardsPerLevel { get; }
}
