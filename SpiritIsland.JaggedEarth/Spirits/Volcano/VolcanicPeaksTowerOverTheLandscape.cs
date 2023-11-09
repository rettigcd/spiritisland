namespace SpiritIsland.JaggedEarth;

public class VolcanicPeaksTowerOverTheLandscape : DefaultRangeCalculator {

	public const string Name = "Volcanic Peaks Tower Over the Landscape";
	static public SpecialRule Rule => new SpecialRule( Name, "Your Power Cards gain +1 range if you have 3 or more presence in the origin land." );

	readonly Spirit _self;

	public VolcanicPeaksTowerOverTheLandscape(Spirit self) { _self = self; }

	public override IEnumerable<SpaceState> GetTargetOptionsFromKnownSource( IEnumerable<SpaceState> source, params TargetCriteria[] targetCriteria ) {
		var spaces = base.GetTargetOptionsFromKnownSource( source, targetCriteria )
			.ToList();

		if(targetCriteria[0] is not InnateTargetCriteria) {
			// Add towers +3 range
			SpaceState[] towers = source
				.Where( s => 3 <= _self.Presence.CountOn(s) )
				.ToArray();
			if(towers.Length > 0)
				spaces.AddRange( base.GetTargetOptionsFromKnownSource( towers, targetCriteria.ExtendRange(1) ) );
		}

		return spaces.Distinct();
	}

	// This class could be used on All Innates to identify them as innates.
	public class InnateTargetCriteria : TargetCriteria {
		public InnateTargetCriteria( int range, Spirit spirit, params string[] filters ) 
			:base( range, spirit, filters ) { }
	}

}