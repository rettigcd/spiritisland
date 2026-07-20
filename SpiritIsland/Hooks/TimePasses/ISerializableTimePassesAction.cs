namespace SpiritIsland;

/// <summary>
/// Implemented by IRunWhenTimePasses types beyond the three well-known singletons
/// (Tokens_ForIsland/Healer/Spirit, handled directly by TimePassesActionRegistry). The first element
/// of the returned array is always the type-tag used to find the matching reader, registered via
/// TimePassesActionRegistry.Register - same shape as ISerializableSpaceEntity/ISerializableInvaderSlot.
/// </summary>
public interface ISerializableTimePassesAction {
	JsonArray ToJson( ISerializationContext ctx );
}
