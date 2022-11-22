namespace SpiritIsland.JaggedEarth;

class VolcanoTargetLandApi : DefaultRangeCalculator {

	public override IEnumerable<Space> GetTargetOptionsFromKnownSource( SelfCtx ctx, TargettingFrom powerType, IEnumerable<SpaceState> source, TargetCriteria targetCriteria ) {
		List<Space> spaces = base.GetTargetOptionsFromKnownSource( ctx, powerType, source, targetCriteria )
			.ToList();

		// Add towers
		if(powerType != TargettingFrom.Innate) {
			var powerUpSpaces = ctx.GameState.AllActiveSpaces
				.Where( s => 3 <= ctx.Self.Presence.CountOn( s ) );
			var towers = ctx.GameState.Tokens.PowerUp( powerUpSpaces.Select(x=>x.Space) ).ToArray();
			if(towers.Length > 0)
				spaces.AddRange( base.GetTargetOptionsFromKnownSource( ctx, powerType, towers, new TargetCriteria( targetCriteria.Range + 1, targetCriteria.Filter ) ) );
		}

		return spaces.Distinct();
	}

	//public override IEnumerable<Space> GetTargetOptionsFromKnownSource( SelfCtx ctx, TargettingFrom powerType, IEnumerable<Space> source, TargetCriteria tc ) {
	//	List<Space> spaces = base.GetTargetOptionsFromKnownSource( ctx, powerType, source, tc )
	//		.ToList();

	//	// Add towers
	//	if(powerType != TargettingFrom.Innate) {
	//		var towers = ctx.Self.Presence.Placed.Where(s=>3 <= ctx.Self.Presence.CountOn(s)).ToArray();
	//		if(towers.Length>0)
	//			spaces.AddRange( base.GetTargetOptionsFromKnownSource( ctx,powerType,towers,new TargetCriteria(tc.Range+1,tc.Filter)) );
	//	}

	//	return spaces.Distinct();
	//}

}