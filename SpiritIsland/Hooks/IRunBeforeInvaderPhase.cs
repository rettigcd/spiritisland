namespace SpiritIsland;

/// <summary>
/// Can be added to GameState and runs before each invader phase. Registered directly on
/// GameState (PreInvaderPhaseActionList), never placed on a Space - not an ISpaceEntity.
/// </summary>
public interface IRunBeforeInvaderPhase {
	/// <returns>If object should be Removed from the TimePasses</returns>
	Task BeforeInvaderPhase( GameState gameState );
	/// <summary> Indicates if action should be removed after running once. </summary>
	bool RemoveAfterRun { get; }
}
