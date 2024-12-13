namespace SpiritIsland;

/// <summary>
/// Calculates Ranges using standard Range Rules
/// Pluggable into Spirit to modify how their Power-Range
/// </summary>
public class DefaultRangeCalculator : ICalcRange {

	public TargetRoutes GetTargetingRoute_MultiSpace(IEnumerable<Space> sources, TargetCriteria targetCriteria) => new TargetRoutes(
		sources.SelectMany(source=>GetTargetingRoute(source,targetCriteria)._routes.Distinct())
	);

	public virtual TargetRoutes GetTargetingRoute(Space source, TargetCriteria tc)
		=> new TargetRoutes( source.Range(tc.Range).Where(tc.Matches).Select(target=>new TargetRoute(source,target)) );

	static public readonly ICalcRange Singleton = new DefaultRangeCalculator();
}

