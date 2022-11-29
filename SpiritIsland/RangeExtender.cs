namespace SpiritIsland;

public class RangeExtender : ICalcRange {

	readonly int extension;
	readonly ICalcRange originalApi;

	public RangeExtender( int extension, ICalcRange originalApi ) {
		this.extension = extension;
		this.originalApi = originalApi;
	}

	public IEnumerable<SpaceState> GetTargetOptionsFromKnownSource(
		Spirit self,
		TerrainMapper terrainMapper,
		TargetingPowerType powerType,
		IEnumerable<SpaceState> source,
		TargetCriteria targetCriteria
	) {
		return originalApi.GetTargetOptionsFromKnownSource( self, terrainMapper, powerType, source, new TargetCriteria( targetCriteria.Range + extension, targetCriteria.Filter ) );
	}

}
