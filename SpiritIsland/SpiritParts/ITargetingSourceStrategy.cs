namespace SpiritIsland;

public interface ITargetingSourceStrategy {
	IEnumerable<Space> EvaluateFrom( IKnowSpiritLocations presence, TargetFrom from );

	/// <summary>
	/// Plain interface member (same precedent as IFearCard.ToJson()/IRunBeforeInvaderPhase.ToJson()) -
	/// safe here since nothing implementing this already has an unrelated ToJson. Resolved via
	/// TargetingSourceStrategyRegistry - see docs/GameSerialization-Roadmap.md section 2's
	/// TargetingSourceStrategy/PowerRangeCalc gap.
	/// </summary>
	JsonArray ToJson( ISerializationContext ctx );
}

