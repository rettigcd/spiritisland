namespace SpiritIsland;

/// <summary>
/// Restores: Source
/// </summary>
public class SourceCalcRestorer( Spirit spirit ) : IRunWhenTimePasses {

	#region IRunWhenTimePasses

	Task IRunWhenTimePasses.TimePasses( GameState _ ) {
		spirit.TargetingSourceStrategy = _original;
		return Task.CompletedTask;
	}

	bool IRunWhenTimePasses.RemoveAfterRun => true;

	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Normal;

	#endregion IRunWhenTimePasses

	readonly ITargetingSourceStrategy _original = spirit.TargetingSourceStrategy;
}

public class RollbackPowerRangeCalcToOriginal(Spirit spirit) 
	: IRunWhenTimePasses
{
	#region IRunWhenTimePasses Imp

	bool IRunWhenTimePasses.RemoveAfterRun => true;
	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Normal;

	Task IRunWhenTimePasses.TimePasses(GameState gameState) {
		var cur = spirit.PowerRangeCalc;
		while(cur.Previous is not null)
			cur = cur.Previous;
		spirit.PowerRangeCalc = cur;
		return Task.CompletedTask;
	}
	#endregion IRunWhenTimePasses Imp

}