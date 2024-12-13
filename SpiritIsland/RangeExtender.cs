namespace SpiritIsland;

public class RangeExtender( int extension, ICalcRange originalApi ) : DefaultRangeCalculator {

	static public void Extend( Spirit spirit, int extension ) {
		spirit.PowerRangeCalc = new RangeExtender( extension, spirit.PowerRangeCalc );
	}

	#region constructor
	#endregion

	public override TargetRoutes GetTargetingRoute(Space source, TargetCriteria tc)
		=> _originalApi.GetTargetingRoute(source, tc.ExtendRange(_extension));

	#region private 
	readonly int _extension = extension;
	readonly ICalcRange _originalApi = originalApi;
	#endregion
}