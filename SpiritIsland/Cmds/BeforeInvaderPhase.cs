namespace SpiritIsland;

public class BeforeInvaderPhase( Func<GameState, Task> _func, bool _remove ) : IRunBeforeInvaderPhase {
	static public BeforeInvaderPhase Each( Func<GameState, Task> func ) => new BeforeInvaderPhase( func, false );

	bool IRunBeforeInvaderPhase.RemoveAfterRun => _remove;

	async Task IRunBeforeInvaderPhase.BeforeInvaderPhase( GameState gameState ) {
		await _func( gameState );
	}
}
