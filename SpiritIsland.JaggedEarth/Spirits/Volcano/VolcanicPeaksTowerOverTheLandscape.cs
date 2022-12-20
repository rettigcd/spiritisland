namespace SpiritIsland.JaggedEarth;

public class VolcanicPeaksTowerOverTheLandscape : DefaultRangeCalculator {

	public const string Name = "Volcanic Peaks Tower Over the Landscape";
	static public SpecialRule Rule => new SpecialRule( Name, "Your Power Cards gain +1 range if you have 3 or more presence in the origin land." );

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