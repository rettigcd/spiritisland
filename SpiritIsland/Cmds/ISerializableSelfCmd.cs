namespace SpiritIsland;

/// <summary>
/// Implemented by IActOn&lt;Spirit&gt; commands that need to round-trip when wrapped by a generic
/// action-factory wrapper (FastSlowAction) rather than resolved by position (GrowthAction, cached per
/// GrowthGroup) or by fixed identity (PowerCard/InnatePower). The first element of the returned array
/// is always the type-tag used to find the matching reader, registered via SelfCmdRegistry.Register -
/// same shape as ISerializableTimePassesAction/ISerializableSpaceEntity.
/// </summary>
public interface ISerializableSelfCmd {
	JsonArray ToJson( ISerializationContext ctx );
}
