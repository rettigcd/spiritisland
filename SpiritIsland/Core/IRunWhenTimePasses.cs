namespace SpiritIsland;

/// <summary>
/// Can be added to GameState and runs during each TimePasses option.
/// </summary>
public interface IRunWhenTimePasses : ISpaceEntity {
	/// <returns>If object should be Removed from the TimePasses</returns>
	Task TimePasses( GameState gameState );
	/// <summary> Indicates if action should be removed after running once. </summary>
	bool RemoveAfterRun { get; }

	TimePassesOrder Order { get; }
}
public enum TimePassesOrder { Early, Normal, Late }

public class TimePassesAction( Func<GameState, Task> _func, bool _remove, TimePassesOrder _order ) : IRunWhenTimePasses {

	#region static factory methods

	static public TimePassesAction Once( Func<GameState,Task> func, TimePassesOrder order = TimePassesOrder.Normal ) => new TimePassesAction( func, true, order );
	static public TimePassesAction Once( Action<GameState> action, TimePassesOrder order = TimePassesOrder.Normal ) => new TimePassesAction( action.AsAsync(), true, order );

	#endregion static factory methods

	#region IRunWhenTimePasses Imp
	bool IRunWhenTimePasses.RemoveAfterRun => _remove;
	async Task IRunWhenTimePasses.TimePasses( GameState gameState ) { await _func( gameState ); }
	TimePassesOrder IRunWhenTimePasses.Order => _order;

	#endregion IRunWhenTimePasses Imp
	#region readonly private fields
	#endregion readonly private fields
}
