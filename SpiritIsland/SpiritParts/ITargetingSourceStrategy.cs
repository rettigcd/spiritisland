namespace SpiritIsland;

public interface ITargetingSourceStrategy {
	IEnumerable<Space> EvaluateFrom( IKnowSpiritLocations presence, TargetFrom from );

	/// <summary>
	/// Plain interface member (same precedent as IFearCard.ToJson()/IRunBeforeInvaderPhase.ToJson()) -
	/// safe here since nothing implementing this already has an unrelated ToJson. Resolved via
	/// TargetingSourceStrategyRegistry.
	/// </summary>
	JsonArray ToJson( ISerializationContext ctx );
}

