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


/// <summary>
/// Can be added to GameState and runs after each invader phase
/// </summary>
public interface IRunAfterInvaderPhase : ISpaceEntity {
	/// <returns>If object should be Removed from the TimePasses</returns>
	Task AfterInvaderPhase( GameState gameState );
	/// <summary> Indicates if action should be removed after running once. </summary>
	bool RemoveAfterRun { get; }
}

public class BeforeInvaderPhase( Func<GameState, Task> _func, bool _remove ) : IRunBeforeInvaderPhase {
	static public BeforeInvaderPhase Each( Func<GameState, Task> func ) => new BeforeInvaderPhase( func, false );

	bool IRunBeforeInvaderPhase.RemoveAfterRun => _remove;

	async Task IRunBeforeInvaderPhase.BeforeInvaderPhase( GameState gameState ) {
		await _func( gameState );
	}
}
