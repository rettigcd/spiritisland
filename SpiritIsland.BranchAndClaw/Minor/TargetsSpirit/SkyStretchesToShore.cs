namespace SpiritIsland.BranchAndClaw;

public class SkyStretchesToShore {

	[MinorCard( "Sky Stretches to Shore", 1, Element.Sun, Element.Air, Element.Water, Element.Earth )]
	[Fast]
	[AnySpirit]
	static public Task ActAsync( TargetSpiritCtx ctx ) {

		// this turn, target spirit may use 1 slow power as if it were fast or vice versa
		ctx.Other.AddActionFactory( new ResolveSlowDuringFast_OrViseVersa() );

		// Target Spirit gains +3 range for targeting costal lands only
		ctx.GameState.TimePasses_ThisRound.Push( new PowerApiRestorer( ctx.Other ).Restore );
		_ = new SkyStretchesToShoreApi( ctx.Other ); // Auto-binds to spirit

		return Task.CompletedTask;
	}

}

class SkyStretchesToShoreApi : DefaultRangeCalculator {
	public SkyStretchesToShoreApi( Spirit spirit ) {
		this.orig = spirit.RangeCalc;
		spirit.RangeCalc = this;
	}

	public override IEnumerable<Space> GetTargetOptionsFromKnownSource( SelfCtx ctx, TargettingFrom powerType, IEnumerable<SpaceState> source, TargetCriteria tc ) {
		var normal = orig.GetTargetOptionsFromKnownSource( ctx, powerType, source, tc );
		var shore = orig.GetTargetOptionsFromKnownSource( ctx, powerType, source, new TargetCriteria(tc.Range+3, tc.Filter) ).Where(x => x.IsCoastal);
		return normal.Union(shore);
	}

	readonly ICalcRange orig;
}