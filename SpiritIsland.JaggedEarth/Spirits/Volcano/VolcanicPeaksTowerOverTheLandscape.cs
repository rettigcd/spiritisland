namespace SpiritIsland.JaggedEarth;

class VolcanicPeaksTowerOverTheLandscape : DefaultRangeCalculator {

	static public readonly SpecialRule Rule = new SpecialRule( "Volcanic Peaks Tower Over the Landscape", "Your Power Cards gain +1 range if you have 3 or more presence in the origin land." );

	// !!! All power cards that target using the Range <== 1 ==> symbol need to use Spirit Ranging, NOT Presence nor BoundPresence ranging. (unless BoundPresence implicitly uses TargetPresence ranging)
	// Blazing Renewal( does the land placement restrictions still apply?)
	// Unrelenting Growth( does the land placement restrictions still apply? )
	// Gift of Proliferation
	// Bargans of Power and Protection
	// Unleash a Torrent of The Self's Own Essence
	// Utter A Curse of Dread And Bone
	// Perils of the Deepest Island


	public override IEnumerable<Space> GetTargetOptionsFromKnownSource( SelfCtx ctx, TargetingPowerType powerType, IEnumerable<SpaceState> source, TargetCriteria targetCriteria ) {
		List<Space> spaces = base.GetTargetOptionsFromKnownSource( ctx, powerType, source, targetCriteria )
			.ToList();

		// Add towers
		if(powerType != TargetingPowerType.Innate) {
			var powerUpSpaces = ctx.GameState.AllActiveSpaces
				.Where( s => 3 <= ctx.Self.Presence.CountOn( s ) );
			var towers = ctx.GameState.Tokens.PowerUp( powerUpSpaces.Select(x=>x.Space) ).ToArray();
			if(towers.Length > 0)
				spaces.AddRange( base.GetTargetOptionsFromKnownSource( ctx, powerType, towers, new TargetCriteria( targetCriteria.Range + 1, targetCriteria.Filter ) ) );
		}

		return spaces.Distinct();
	}

}