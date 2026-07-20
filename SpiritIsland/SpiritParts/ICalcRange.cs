namespace SpiritIsland;

public interface ICalcRange {
	TargetRoutes GetTargetingRoute_MultiSpace(IEnumerable<Space> sources, TargetCriteria targetCriteria);
	TargetRoutes GetTargetingRoute(Space source, TargetCriteria tc);
	/// <summary>
	/// The previous one that we restore to when done with it.
	/// </summary>
	ICalcRange? Previous { get; }

	/// <summary>
	/// Plain interface member, same precedent as ITargetingSourceStrategy.ToJson(). Resolved via
	/// RangeCalcRegistry - see docs/GameSerialization-Roadmap.md section 2's
	/// TargetingSourceStrategy/PowerRangeCalc gap.
	/// </summary>
	JsonArray ToJson( ISerializationContext ctx );
}
