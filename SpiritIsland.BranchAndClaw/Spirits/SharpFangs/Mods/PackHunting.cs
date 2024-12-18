namespace SpiritIsland.BranchAndClaw;

public class PackHunting : DefaultRangeCalculator {
	public const string Name = "Pack Hunting";
	const string Description = "Your Powers get +1 Range for targeting lands with Beasts.";
	static public SpecialRule Rule => new SpecialRule(Name, Description);

	public override TargetRoutes GetTargetingRoute(Space source, TargetCriteria tc) {
		TargetRoutes routes = Previous!.GetTargetingRoute(source, tc);
		routes.AddRoutes(
			Previous.GetTargetingRoute(source, tc.ExtendRange(1))._routes
				.Where(x => x.target.Beasts.Any)
		);
		return routes;
	}

}
