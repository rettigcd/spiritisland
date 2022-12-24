namespace SpiritIsland.JaggedEarth;

public class VolcanicPeaksTowerOverTheLandscape : DefaultRangeCalculator {

	public const string Name = "Volcanic Peaks Tower Over the Landscape";
	static public SpecialRule Rule => new SpecialRule( Name, "Your Power Cards gain +1 range if you have 3 or more presence in the origin land." );

	readonly Spirit _self;

	public VolcanicPeaksTowerOverTheLandscape(Spirit self) { _self = self; }

	public override IEnumerable<SpaceState> GetTargetOptionsFromKnownSource( TargetingPowerType powerType, IEnumerable<SpaceState> source, TargetCriteria targetCriteria ) {
		var spaces = base.GetTargetOptionsFromKnownSource( powerType, source, targetCriteria )
			.ToList();

		// Add towers
		if( targetCriteria is not InnateTargetCriteria ) {
			var towers = source
				.Where( s => 3 <= _self.Presence.CountOn( s ) )
				.ToArray();
			if(towers.Length > 0)
				spaces.AddRange( base.GetTargetOptionsFromKnownSource( powerType, towers, targetCriteria.ExtendRange(1) ) );
		}

		return spaces.Distinct();
	}

	public class InnateTargetCriteria : TargetCriteria {
		public InnateTargetCriteria( TerrainMapper terrainMapper, int range, params string[] filters ) 
			:base( terrainMapper, range, filters ) { }
	}

}