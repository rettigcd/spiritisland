
namespace SpiritIsland;

/// <summary>
/// Mod that inserts Slow cards into the ActionList during EACH Fast phase.
/// </summary>
abstract public class RunSlowCardsAsFastMod_EveryRound(Spirit spirit) : IModifyAvailableActions, IHandleActivatedActions, ICleanupSpiritWhenTimePasses {

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

	void ICleanupSpiritWhenTimePasses.CleanupSpirit( Spirit spirit ) => _usedCount = 0;

	#region protected

	protected abstract int AllowedCount { get; }

	/// <summary> Exposed so each concrete subclass can implement ISerializableSpiritMod against its own
	/// tag, capturing/restoring just this plain counter - no reference-equality concern here, unlike
	/// _slowAsFast (rebuilt fresh from AllActions every Modify() call, never itself persisted). </summary>
	protected int UsedCountForJson { get => _usedCount; set => _usedCount = value; }

	protected virtual bool EvaluateAction(IActionFactory slowAction) {
		return slowAction.CouldActivateDuring(Phase.Slow, _spirit);
	}

	readonly protected Spirit _spirit = spirit;

	#endregion

	#region private fields

	IActionFactory[] _slowAsFast = [];

	int _usedCount;

	#endregion private fields

}