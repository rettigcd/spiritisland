namespace SpiritIsland;

/// <summary>
/// Can be added to GameState and runs during each TimePasses option.
/// </summary>
public interface IRunWhenTimePasses : ISpaceEntity {
	/// <returns>If object should be Removed from the TimePasses</returns>
	Task TimePasses( GameState gameState );
	/// <summary> Indicates if action should be removed after running once. </summary>
	bool RemoveAfterRun { get; }
}


public class TimePassesOnce : IRunWhenTimePasses {

	public TimePassesOnce(Func<GameState,Task> func ) {
		_func = func;
	}
	public TimePassesOnce( Action<GameState> action ) {
		_func = (gs) => { action(gs); return Task.CompletedTask; };
	}

	bool IRunWhenTimePasses.RemoveAfterRun => true;

	async Task IRunWhenTimePasses.TimePasses( GameState gameState ){
		await _func(gameState);
	}
	readonly Func<GameState,Task> _func;
}
