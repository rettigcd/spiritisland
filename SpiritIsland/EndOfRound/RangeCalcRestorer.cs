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

public class SourceCalcRestorer {

	readonly Spirit spirit;
	readonly ITargetingSourceStrategy original;
	public SourceCalcRestorer(Spirit spirit ) {
		this.spirit = spirit;
		this.original = spirit.TargetingSourceStrategy; // capture so we can put it back later
	}
	public Task Restore( GameState _ ) {
		spirit.TargetingSourceStrategy = original;
		return Task.CompletedTask;
	}

}

public class RangeCalcRestorer {

	static public void Save( Spirit spirit ) {
		GameState.Current.TimePasses_ThisRound.Push( new RangeCalcRestorer( spirit ).Restore );
	}

	readonly Spirit spirit;
	readonly ICalcRange original;
	public RangeCalcRestorer(Spirit spirit ) {
		this.spirit = spirit;
		this.original = spirit.PowerRangeCalc; // capture so we can put it back later
	}
	public Task Restore( GameState _ ) {
		spirit.PowerRangeCalc = original;
		return Task.CompletedTask;
	}

}