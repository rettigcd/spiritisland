namespace SpiritIsland;

public class RangeExtender : ICalcPowerRange {

	readonly int extension;
	readonly ICalcPowerRange originalApi;

	public RangeExtender( int extension, ICalcPowerRange originalApi ) {
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

	//public IEnumerable<Space> GetTargetOptionsFromKnownSource( Spirit self, GameState gameState, TargettingFrom powerType, IEnumerable<Space> source, TargetCriteria tc ) {
	//	return originalApi.GetTargetOptionsFromKnownSource( self, gameState, powerType, source, new TargetCriteria( tc.Range + extension, tc.Filter) );
	//}

}
