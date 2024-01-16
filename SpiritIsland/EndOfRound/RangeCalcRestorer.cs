namespace SpiritIsland;

//public class RangeCalcRestorer {

//	readonly Spirit spirit;
//	readonly ICalcRange original;
//	public RangeCalcRestorer(Spirit spirit ) {
//		this.spirit = spirit;
//		this.original = spirit.RangeCalc; // capture so we can put it back later
//	}
//	public Task Restore( GameState _ ) {
//		spirit.RangeCalc = original;
//		return Task.CompletedTask;
//	}

//}

public class SourceCalcRestorer : IRunWhenTimePasses {

	readonly Spirit spirit;
	readonly ITargetingSourceStrategy original;
	public SourceCalcRestorer(Spirit spirit ) {
		this.spirit = spirit;
		this.original = spirit.TargetingSourceStrategy; // capture so we can put it back later
	}
	Task IRunWhenTimePasses.TimePasses( GameState _ ) {
		spirit.TargetingSourceStrategy = original;
		return Task.CompletedTask;
	}
	bool IRunWhenTimePasses.RemoveAfterRun => true;
	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Normal;

	public Task TimePasses( GameState gameStTate ) => throw new NotImplementedException();
}

public class RangeCalcRestorer : IRunWhenTimePasses {

	static public void Save( Spirit spirit ) {
		GameState.Current.AddTimePassesAction( new RangeCalcRestorer( spirit ) );
	}

	readonly Spirit spirit;
	readonly ICalcRange original;
	public RangeCalcRestorer(Spirit spirit ) {
		this.spirit = spirit;
		this.original = spirit.PowerRangeCalc; // capture so we can put it back later
	}
	Task IRunWhenTimePasses.TimePasses( GameState _ ) {
		spirit.PowerRangeCalc = original;
		return Task.CompletedTask;
	}
	TimePassesOrder IRunWhenTimePasses.Order => TimePassesOrder.Normal;
	bool IRunWhenTimePasses.RemoveAfterRun => true;


}