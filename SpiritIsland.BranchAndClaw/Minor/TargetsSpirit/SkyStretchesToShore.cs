namespace SpiritIsland.BranchAndClaw;

public class SkyStretchesToShore {

	[MinorCard( "Sky Stretches to Shore", 1, Element.Sun, Element.Air, Element.Water, Element.Earth )]
	[Fast]
	[AnySpirit]
	static public Task ActAsync( TargetSpiritCtx ctx ) {

		// this turn, target spirit may use 1 slow power as if it were fast or vice versa
		ctx.Other.AddActionFactory( new ResolveSlowDuringFast_OrViseVersa() );

		// Target Spirit gains +3 range for targeting costal lands only
		RangeCalcRestorer.Save(ctx.Other,ctx.GameState);
		_ = new SkyStretchesToShoreApi( ctx.Other ); // Auto-binds to spirit

		return Task.CompletedTask;
	}

}

class SkyStretchesToShoreApi : DefaultRangeCalculator {
	public SkyStretchesToShoreApi( Spirit spirit ) {
		this.orig = spirit.PowerRangeCalc;
		spirit.PowerRangeCalc = this;
	}

	public override IEnumerable<SpaceState> GetTargetOptionsFromKnownSource( Spirit self, TargetingPowerType powerType, IEnumerable<SpaceState> source, TargetCriteria tc ) {
		var normal = orig.GetTargetOptionsFromKnownSource( self, powerType, source, tc );
		var shore = orig.GetTargetOptionsFromKnownSource( self, powerType, source, tc.ExtendRange(3) )
			.Where(x => x.Space.IsCoastal);
		return normal.Union(shore);
	}

	readonly ICalcRange orig;
}