namespace SpiritIsland.Basegame;

class IncludeALandRangeCalculator(Spirit spirit, ICalcRange previous, Space target) : DefaultRangeCalculator(previous) {

	public override TargetRoutes GetTargetingRoute(Space source, TargetCriteria tc) {
		var routes = Previous.GetTargetingRoute(source, tc);
		routes.AddRoutes(RoutesToTarget(tc));
		return routes;
	}

	IEnumerable<TargetRoute> RoutesToTarget(TargetCriteria tc) => spirit.Presence.Lands
		.Where(tc.Matches)
		.Select(s => new TargetRoute(s, target));

}