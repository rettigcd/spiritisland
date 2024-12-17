
namespace SpiritIsland.Basegame;

/// <summary>
/// Mod that inserts Slow cards into the ActionList during EACH Fast phase.
/// </summary>
abstract class RunSlowCardsAsFastMod_EveryRound(Spirit spirit) : IModifyAvailableActions, IHandleActivatedActions {

	#region IModifyAvailableActions

	void IModifyAvailableActions.Modify(List<IActionFactory> orig, Phase phase) {
		bool canMakeSlowFast = phase == Phase.Fast && _usedCount < AllowedCount;
		if( !canMakeSlowFast ) return;

		_slowAsFast = _spirit.AllActions.Where(EvaluateAction)
			.Except(orig) // in case another mod added slow-as-fast, don't re-add it.
			.ToArray();
		orig.AddRange(_slowAsFast);
	}

	#endregion IModifyAvailableActions

	void IHandleActivatedActions.ActionActivated(IActionFactory factory) {
		if( _slowAsFast.Contains(factory) )
			++_usedCount;
	}

	#region protected

	protected abstract int AllowedCount { get; }

	protected virtual bool EvaluateAction(IActionFactory slowAction) {
		return slowAction.CouldActivateDuring(Phase.Slow, _spirit);
	}

	readonly protected Spirit _spirit = spirit;

	#endregion

	#region private fields

	IActionFactory[] _slowAsFast = [];

	int _usedCount {
		get => GameState.Current.RoundScope.TryGetValue(UsedKey,out object val) ? (int)val : 0;
		set => GameState.Current.RoundScope[UsedKey] = value;
	}
	string UsedKey => _usedKey ??= "SlowAsFast:" + Guid.NewGuid().ToString(); string _usedKey;

	#endregion private fields

}
