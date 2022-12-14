namespace SpiritIsland;

public interface IAdversary {
	int Level { set; }
	int[] InvaderCardOrder { get; }
	int[] FearCardsPerLevel { get; }
	void PreInitialization( GameState gameState );
	void PostInitialization( GameState gamestate );

	public ScenarioLevel[] Adjustments { get; }
}

public class ScenarioLevel {

	public ScenarioLevel( int difficulty, int fear1, int fear2, int fear3, string title, string description ) {
		Difficulty = difficulty;
		FearCards = new int[] { fear1, fear2, fear3 };
		Title = title;
		Description = description;
	}

	public int Difficulty { get; }
	public int[] FearCards { get; }
	public string Title { get; }
	public string Description { get; }
}