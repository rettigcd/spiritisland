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

public class TimePassesAction : IRunWhenTimePasses {

	static public TimePassesAction Once( Func<GameState,Task> func ) => new TimePassesAction( func, true );
	static public TimePassesAction Once( Action<GameState> action ) => new TimePassesAction( action.AsAsync(), true );

	public TimePassesAction( Func<GameState, Task> func, bool remove ) { _func = func; _remove = remove; }
	bool IRunWhenTimePasses.RemoveAfterRun => _remove;

	async Task IRunWhenTimePasses.TimePasses( GameState gameState ) {
		await _func( gameState );
	}
	readonly Func<GameState, Task> _func;
	readonly bool _remove;
}
