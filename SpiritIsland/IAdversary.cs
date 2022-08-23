namespace SpiritIsland;

public interface IAdversary {
	int Level { set; }
	int[] InvaderCardOrder { get; }
	int[] FearCardsPerLevel { get; }
	void PreInitialization( GameState gameState );
	void PostInitialization( GameState gamestate );
}
