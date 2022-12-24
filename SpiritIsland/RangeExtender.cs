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

	public IEnumerable<SpaceState> GetTargetOptionsFromKnownSource( IEnumerable<SpaceState> source, TargetCriteria targetCriteria )
		=> originalApi.GetTargetOptionsFromKnownSource( source, targetCriteria.ExtendRange( extension ) );
	

}
