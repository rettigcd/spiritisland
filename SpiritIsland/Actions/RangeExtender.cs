namespace SpiritIsland;

public class RangeExtender : ICalcRange {

	readonly int extension;
	readonly ICalcRange originalApi;

	public RangeExtender( int extension, ICalcRange originalApi ) {
		this.extension = extension;
		this.originalApi = originalApi;
	}

	public IEnumerable<Space> GetTargetOptionsFromKnownSource(
		SelfCtx ctx,
		TargetingPowerType powerType,
		IEnumerable<SpaceState> source,
		TargetCriteria targetCriteria
	) {
		return originalApi.GetTargetOptionsFromKnownSource( ctx, powerType, source, new TargetCriteria( targetCriteria.Range + extension, targetCriteria.Filter ) );
	}

}
