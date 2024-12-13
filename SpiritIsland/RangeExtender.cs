namespace SpiritIsland;

public class RangeExtender( int extension, ICalcRange originalApi ) : DefaultRangeCalculator(originalApi) {

	static public void Extend( Spirit spirit, int extension ) {
		spirit.PowerRangeCalc = new RangeExtender( extension, spirit.PowerRangeCalc );
	}

	public override TargetRoutes GetTargetingRoute(Space source, TargetCriteria tc)
		=> Previous.GetTargetingRoute(source, tc.ExtendRange(_extension));

	#region private 
	readonly int _extension = extension;
	#endregion
}