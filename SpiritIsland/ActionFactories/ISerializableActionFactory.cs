namespace SpiritIsland;

/// <summary>
/// Implemented by IActionFactory types beyond the well-known PowerCard/InnatePower/GrowthAction/
/// FastSlowAction cases Spirit.SerializeActionFactory handles directly. The first element of the
/// returned array is always the type-tag used to find the matching reader, registered via
/// ActionFactoryRegistry.Register - same shape as ISerializableSelfCmd/ISerializableTimePassesAction.
/// </summary>
public interface ISerializableActionFactory {
	JsonArray ToJson( ISerializationContext ctx );
}
