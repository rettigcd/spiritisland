namespace SpiritIsland;

/// <summary>
/// Calculates Ranges using standard Range Rules
/// Pluggable into Spirit to modify how their Power-Range
/// </summary>
public class DefaultRangeCalculator : ICalcRange {

	/// <summary> Constructs the default ICalcRange for some spirit.  (no Previous value) </summary>
	public DefaultRangeCalculator() { _previous=null;}
	/// <summary> Constructs a temporar ICalcRange that can be rolled back to a previous value. </summary>
	protected DefaultRangeCalculator(ICalcRange previous) { _previous = previous; }

	public TargetRoutes GetTargetingRoute_MultiSpace(IEnumerable<Space> sources, TargetCriteria targetCriteria) => new TargetRoutes(
		sources.SelectMany(source=>GetTargetingRoute(source,targetCriteria)._routes.Distinct())
	);

	public virtual TargetRoutes GetTargetingRoute(Space source, TargetCriteria tc)
		=> new TargetRoutes( source.Range(tc.Range).Where(tc.Matches).Select(target=>new TargetRoute(source,target)) );

	public ICalcRange Previous => _previous;
	readonly ICalcRange _previous;

	static public readonly ICalcRange Singleton = new DefaultRangeCalculator();

}

