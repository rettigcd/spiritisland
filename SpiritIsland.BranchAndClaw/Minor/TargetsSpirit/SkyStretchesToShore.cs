namespace SpiritIsland.BranchAndClaw;

public class SkyStretchesToShore {

	[MinorCard( "Sky Stretches to Shore", 1, Element.Sun, Element.Air, Element.Water, Element.Earth ),Fast,AnySpirit]
	[Instructions( "This turn, target Spirit may use 1 Slow Power as if it were Fast, or vice versa. Target Spirit gains +3 Range for targeting Coastal lands only." ), Artist( Artists.JoshuaWright )]
	static public Task ActAsync( TargetSpiritCtx ctx ) {

		// this turn, target spirit may use 1 slow power as if it were fast or vice versa
		ctx.Other.AddActionFactory( new ResolveSlowDuringFast_OrViseVersa() );

		// Target Spirit gains +3 range for targeting coastal lands only
		RangeCalcRestorer.Save(ctx.Other);
		_ = new SkyStretchesToShoreApi( ctx.Other ); // Auto-binds to spirit

		return Task.CompletedTask;
	}

}

class SkyStretchesToShoreApi : DefaultRangeCalculator {
	public SkyStretchesToShoreApi( Spirit self ) {
		// _self = self;
		_orig = self.PowerRangeCalc;
		self.PowerRangeCalc = this;
	}

	public override IEnumerable<SpaceState> GetTargetOptionsFromKnownSource( 
		IEnumerable<SpaceState> source, 
		TargetCriteria tc 
	) {
		var normal = _orig.GetTargetOptionsFromKnownSource( source, tc );
		var shore = _orig.GetTargetOptionsFromKnownSource( source, tc.ExtendRange(3) )
			.Where(x => x.Space.IsCoastal);
		return normal.Union(shore);
	}

	// readonly Spirit _self;
	readonly ICalcRange _orig;
}