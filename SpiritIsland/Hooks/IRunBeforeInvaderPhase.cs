namespace SpiritIsland;

/// <summary>
/// Can be added to GameState and runs after each invader phase
/// </summary>
public interface IRunBeforeInvaderPhase : ISpaceEntity {
	/// <returns>If object should be Removed from the TimePasses</returns>
	Task BeforeInvaderPhase( GameState gameState );
	/// <summary> Indicates if action should be removed after running once. </summary>
	bool RemoveAfterRun { get; }
}
