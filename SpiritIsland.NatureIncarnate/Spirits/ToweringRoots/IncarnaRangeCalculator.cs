namespace SpiritIsland.NatureIncarnate;

/// <summary>
/// Extend Range+1 if source is Incarna
/// </summary>
public class IncarnaRangeCalculator : DefaultRangeCalculator {

	readonly Incarna _incarna;

	public IncarnaRangeCalculator( Spirit self ) { 
		_incarna = self.Incarna;
	}

	public override IEnumerable<SpaceState> GetSpaceOptions( SpaceState source, TargetCriteria tc ) 
		=> base.GetSpaceOptions( source, source == _incarna.Space ? tc.ExtendRange(1) : tc );

}
