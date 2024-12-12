
namespace SpiritIsland.Basegame;

abstract class RunSlowCardsAsFast(Spirit spirit) : IModifyAvailableActions, IRunWhenTimePasses, IHandleActivatedActions {

	#region IModifyAvailableActions

	void IModifyAvailableActions.Modify(List<IActionFactory> orig, Phase phase) {
		bool canMakeSlowFast = phase == Phase.Fast && _usedAirForFastCount < AllowedCount;
		if( !canMakeSlowFast ) return;
		_slowAsFast = _spirit.AllActions.Where(slowAction => slowAction.CouldActivateDuring(Phase.Slow, _spirit))
			.Except(orig) // in case another mod added slow-as-fast, don't re-add it.
			.ToArray();
		orig.AddRange(_slowAsFast);
	}

	#endregion IModifyAvailableActions

	void IHandleActivatedActions.ActionActivated(IActionFactory factory) {
		if( _slowAsFast.Contains(factory) )
			++_usedAirForFastCount;
	}

	#region IRunWhenTimePasses

	bool IRunWhenTimePasses.RemoveAfterRun => false;
	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Normal;
	Task IRunWhenTimePasses.TimePasses(GameState gameState) {
		_usedAirForFastCount = 0;
		return Task.CompletedTask;
	}

	#endregion IRunWhenTimePasses

	protected abstract int AllowedCount { get; }

	#region private fields

	readonly protected Spirit _spirit = spirit;
	int _usedAirForFastCount = 0;

	IActionFactory[] _slowAsFast = [];

	#endregion private fields

}
