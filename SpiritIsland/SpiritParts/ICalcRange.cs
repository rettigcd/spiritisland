namespace SpiritIsland;

public interface ICalcRange {
	TargetRoutes GetTargetingRoute_MultiSpace(IEnumerable<Space> sources, TargetCriteria targetCriteria);
	TargetRoutes GetTargetingRoute(Space source, TargetCriteria tc);
}

