namespace SpiritIsland;

public interface IAdversary {

	IAdversaryBuilder Builder { get; }

	string Name { get; }
	int Level { get; }
	AdversaryLevel[] ActiveLevels { get; }

	InvaderDeckBuilder InvaderDeckBuilder { get; }

	void AdjustFearCardCounts(int[] counts);

	/// <summary> Decks are already built, but tokens have not been placed yet. </summary>
	void Init( GameState gameState );

	/// <summary> Adjusts Tokens that are already placed. </summary>
	void AdjustPlacedTokens( GameState gamestate );

	public string Describe();

	/// <summary>
	/// {Name, Level} - Level is fixed at construction and never reassigned (see
	/// docs/GameSerialization-Roadmap.md section 9), so this fully captures identity. Round-trips via
	/// AdversaryConfig.ToJson()/FromJson() and AdversaryRegistry.Build(config).
	/// </summary>
	AdversaryConfig Config => new( Name, Level );
}
