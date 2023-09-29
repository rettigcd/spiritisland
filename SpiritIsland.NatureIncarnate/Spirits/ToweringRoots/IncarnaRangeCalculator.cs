namespace SpiritIsland.NatureIncarnate;

public class IncarnaRangeCalculator : DefaultRangeCalculator {

	readonly Spirit _self;
	readonly IIncarnaToken _incarna;

	public IncarnaRangeCalculator( Spirit self ) { 
		_self = self;
		_incarna = ((IHaveIncarna)_self.Presence).Incarna;
	}

	public override IEnumerable<SpaceState> GetTargetOptionsFromKnownSource( IEnumerable<SpaceState> source, TargetCriteria targetCriteria ) {
		var spaces = base.GetTargetOptionsFromKnownSource( source, targetCriteria )
			.ToList();

		if(_incarna.Space != null)
			spaces.AddRange( base.GetTargetOptionsFromKnownSource(new SpaceState[]{ _incarna.Space }, targetCriteria.ExtendRange( 1 ) ) );

		return spaces.Distinct();
	}

}