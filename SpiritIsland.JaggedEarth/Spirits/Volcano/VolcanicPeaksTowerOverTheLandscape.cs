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


	public override IEnumerable<SpaceState> GetTargetOptionsFromKnownSource( Spirit self, TerrainMapper tm, TargetingPowerType powerType, IEnumerable<SpaceState> source, TargetCriteria targetCriteria ) {
		var spaces = base.GetTargetOptionsFromKnownSource( self, tm, powerType, source, targetCriteria )
			.ToList();

		// Add towers
		if(powerType != TargetingPowerType.Innate) {
			var towers = source
				.Where( s => 3 <= self.Presence.CountOn( s ) )
				.ToArray();
			if(towers.Length > 0)
				spaces.AddRange( base.GetTargetOptionsFromKnownSource( self, tm, powerType, towers, targetCriteria.ExtendRange(1) ) );
		}

		return spaces.Distinct();
	}

}