namespace SpiritIsland.NatureIncarnate;

public class ShadowTouchedRealm_RangeCalculator : DefaultRangeCalculator {

	static public SpecialRule Rule => new SpecialRule(
		"Shadow-Touched Realm", 
		"You land-targeting Powers can target Incarna as if it were a land ignoring Range.  EndlessDark is Inland and has no terrain."
	);

	public ShadowTouchedRealm_RangeCalculator() {}

	public override TargetRoutes GetTargetingRoute(Space source, TargetCriteria targetCriteria) {
		return new TargetRoutes(
			base.GetTargetingRoute(source, targetCriteria)._routes.Append(new TargetRoute(source,EndlessDark.Space.ScopeSpace))
		);
	}

}
