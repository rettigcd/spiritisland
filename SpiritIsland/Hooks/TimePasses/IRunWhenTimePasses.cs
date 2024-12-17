namespace SpiritIsland;

/// <summary>
/// Custom time-passes actions that are not scoped to a Space nor a Spirit
/// </summary>
public interface IRunWhenTimePasses {

	/// <returns>If object should be Removed from the TimePasses</returns>
	Task TimePasses( GameState gameState );

	/// <summary> Indicates if action should be removed after running once. </summary>
	bool RemoveAfterRun { get; }

	TimePassesOrder Order { get; }

}
public enum TimePassesOrder { Early, Normal, Late }

public class TimePassesAction( Func<GameState, Task> _func, bool _remove, TimePassesOrder _order ) : IRunWhenTimePasses {

	static public TimePassesAction Once( Func<GameState,Task> func, TimePassesOrder order = TimePassesOrder.Normal ) => new TimePassesAction( func, true, order );
	static public TimePassesAction Once( Action<GameState> action, TimePassesOrder order = TimePassesOrder.Normal ) => new TimePassesAction( action.AsAsync(), true, order );

	#region IRunWhenTimePasses Imp
	bool IRunWhenTimePasses.RemoveAfterRun => _remove;
	Task IRunWhenTimePasses.TimePasses( GameState gameState ) => _func( gameState );
	TimePassesOrder IRunWhenTimePasses.Order => _order;
	#endregion IRunWhenTimePasses Imp

}
