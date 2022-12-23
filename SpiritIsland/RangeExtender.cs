namespace SpiritIsland;

public class RangeExtender : ICalcRange {

	readonly int extension;
	readonly ICalcRange originalApi;

	static public void Extend( Spirit spirit, int extension ) {
		spirit.PowerRangeCalc = new RangeExtender( extension, spirit.PowerRangeCalc );
	}

	public RangeExtender( int extension, ICalcRange originalApi ) {
		this.extension = extension;
		this.originalApi = originalApi;
	}

	public IEnumerable<SpaceState> GetTargetOptionsFromKnownSource(
		Spirit self,
		TargetingPowerType powerType,
		IEnumerable<SpaceState> source,
		TargetCriteria targetCriteria
	) {
		return originalApi.GetTargetOptionsFromKnownSource( self, powerType, source, targetCriteria.ExtendRange( extension ) );
	}

}
