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

public class TimePassesAction : IRunWhenTimePasses {

	#region static factory methods

	static public TimePassesAction Once( Func<GameState,Task> func ) => new TimePassesAction( func, true, TimePassesOrder.Normal );
	static public TimePassesAction Once( Action<GameState> action ) => new TimePassesAction( action.AsAsync(), true, TimePassesOrder.Normal );

	#endregion static factory methods

	public TimePassesAction( Func<GameState, Task> func, bool remove, TimePassesOrder order ) { 
		_func = func; 
		_remove = remove;
		_order = order;
	}

	#region IRunWhenTimePasses Imp
	bool IRunWhenTimePasses.RemoveAfterRun => _remove;
	async Task IRunWhenTimePasses.TimePasses( GameState gameState ) { await _func( gameState ); }
	TimePassesOrder IRunWhenTimePasses.Order => _order;
	#endregion IRunWhenTimePasses Imp

	#region readonly private fields
	readonly Func<GameState, Task> _func;
	readonly bool _remove;
	readonly TimePassesOrder _order;
	#endregion readonly private fields
}
