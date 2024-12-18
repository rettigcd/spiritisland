namespace SpiritIsland.BranchAndClaw;

public class SkyStretchesToShore {

	[MinorCard( "Sky Stretches to Shore", 1, Element.Sun, Element.Air, Element.Water, Element.Earth ),Fast,AnySpirit]
	[Instructions( "This turn, target Spirit may use 1 Slow Power as if it were Fast, or vice versa. Target Spirit gains +3 Range for targeting Coastal lands only." ), Artist( Artists.JoshuaWright )]
	static public Task ActAsync( TargetSpiritCtx ctx ) {

		// this turn, target spirit may use 1 slow power as if it were fast or vice versa
		ctx.Other.AddActionFactory( new ResolveSlowDuringFast_OrViseVersa() );

		// Target Spirit gains +3 range for targeting coastal lands only
		ctx.Other.PowerRangeCalc = new SkyStretchesToShoreApi( ctx.Other );

		return Task.CompletedTask;
	}

}

class SkyStretchesToShoreApi(Spirit self) : DefaultRangeCalculator(self.PowerRangeCalc) {

	public override TargetRoutes GetTargetingRoute(Space source, TargetCriteria tc) {
		var normal = Previous!.GetTargetingRoute(source, tc);
		var shore = Previous!.GetTargetingRoute(source, tc.ExtendRange(3))._routes
			.Where(x => x.target.SpaceSpec.IsCoastal);
		return new TargetRoutes( normal._routes.Union(shore) );
	}

}