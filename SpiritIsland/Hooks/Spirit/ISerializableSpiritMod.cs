namespace SpiritIsland;

/// <summary>
/// Implemented by ISpiritMod types that carry extra state beyond what replaying spirit/aspect
/// construction already gives them for free (docs/ISpiritMod-Types.md's Medium/High tiers - Low tier
/// needs no interface at all, see SpiritMods_LowTier_Tests). The first element of the returned array is
/// always the type-tag used to find the matching restore action, registered via
/// SpiritModRegistry.Register - same shape as ISerializableSelfCmd/ISerializableTimePassesAction.
/// </summary>
public interface ISerializableSpiritMod {
	JsonArray ToJson( ISerializationContext ctx );
}
