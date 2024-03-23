namespace SpiritIsland.SinglePlayer;

/// <summary>
/// Hides from the caler the nature of the game engine.  They don't know about throwing exceptions and cancelation tokens.
/// </summary>
public interface IGamePortal {
	IDecisionPortal DecisionPortal { get; }
	void RewindToRound( int targetRound );
	void CancelGame();
}
