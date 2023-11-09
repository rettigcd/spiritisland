namespace SpiritIsland.NatureIncarnate;

public class IncarnaRangeCalculator : DefaultRangeCalculator {

	readonly Spirit _self;
	readonly IIncarnaToken _incarna;

	public IncarnaRangeCalculator( Spirit self ) { 
		_self = self;
		_incarna = ((IHaveIncarna)_self.Presence).Incarna;
	}

	public override IEnumerable<SpaceState> GetTargetOptionsFromKnownSource( IEnumerable<SpaceState> source, params TargetCriteria[] targetCriteria ) {
		var spaces = base.GetTargetOptionsFromKnownSource( source, targetCriteria )
			.ToList();

		if(IncarnaInSource( source ))
			spaces.AddRange(
				base.GetTargetOptionsFromKnownSource(
					new SpaceState[] { _incarna.Space! },
					targetCriteria.ExtendRange( 1 )
				)
			);

		return spaces.Distinct();
	}

	bool IncarnaInSource( IEnumerable<SpaceState> source ) => _incarna.Space != null && source.Contains( _incarna.Space );
}
