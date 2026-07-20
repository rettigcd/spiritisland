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

	public ICalcRange? Previous => _previous;
	readonly ICalcRange? _previous;

	static public readonly ICalcRange Singleton = new DefaultRangeCalculator();

	/// <summary>
	/// Only ever reached by a bare DefaultRangeCalculator (Previous == null - the protected ctor is only
	/// callable by the 4 named temporary-decorator subclasses, each of which overrides this with its own
	/// tag/extra state instead).
	/// </summary>
	public virtual JsonArray ToJson( ISerializationContext ctx ) => new JsonArray( Tag );

	const string Tag = "Default";

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> RangeCalcRegistry.Register( Tag, ( json, ctx ) => Singleton );

}

