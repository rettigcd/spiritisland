namespace SpiritIsland.NatureIncarnate;

/// <summary>
/// Extend Range+1 if source is Incarna
/// </summary>
public class IncarnaRangeCalculator : DefaultRangeCalculator {

	readonly Spirit _self;
	readonly IIncarnaToken _incarna;

	public IncarnaRangeCalculator( Spirit self ) { 
		_self = self;
		_incarna = ((IHaveIncarna)_self.Presence).Incarna;
	}

	public override IEnumerable<SpaceState> GetSpaceOptions( SpaceState source, TargetCriteria tc ) 
		=> base.GetSpaceOptions( source, source == _incarna.Space ? tc.ExtendRange(1) : tc );

}
