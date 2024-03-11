namespace SpiritIsland.JaggedEarth;

public class VolcanicPeaksTowerOverTheLandscape( Spirit _self ) : DefaultRangeCalculator {

	public const string Name = "Volcanic Peaks Tower Over the Landscape";
	static public SpecialRule Rule => new SpecialRule( Name, "Your Power Cards gain +1 range if you have 3 or more presence in the origin land." );

	public override IEnumerable<Space> GetSpaceOptions( Space source, TargetCriteria tc ) {
		if(tc is not InnateTargetCriteria && 3 <= _self.Presence.CountOn( source ))
			tc = tc.ExtendRange( 1 );
		return base.GetSpaceOptions( source, tc );
	}

	// This class could be used on All Innates to identify them as innates.
	public class InnateTargetCriteria( int range, Spirit spirit, params string[] filters ) 
		: TargetCriteria( range, spirit, filters ) 
	{}

}