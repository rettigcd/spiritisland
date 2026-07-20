namespace SpiritIsland;

/// <summary>
/// Implemented by ISpiritMod types whose own IActionFactory field(s) get compared against
/// spirit.UsedActions/AllActions by reference later (docs/ISpiritMod-Types.md's High tier -
/// ShadowsPartakeOfAmorphousSpace, PourDownPower, MistsSteadilyDrift, StrandedInTheShiftingMists,
/// UnrelentingStrides, MarkedBeastMover). Spirit.SerializeActionFactory/DeserializeActionFactory checks
/// every Mods entry implementing this FIRST, before falling through to the ordinary PowerCard/
/// InnatePower/GrowthAction/etc. cases - because the owning mod (already re-added for free by
/// spirit/aspect construction replay, same as the Low/Medium tiers) must resolve back to the exact same
/// cached instance it already holds, not a fresh, reference-distinct one built from the factory's own
/// serialized content.
/// </summary>
public interface IOwnedActionFactories {
	/// <summary> Stable per-mod-type tag - same role as every other registry's Tag constant in this
	/// project, just looked up through spirit.Mods instead of a static dictionary, since the instance to
	/// resolve back to already lives there. </summary>
	string ModTag { get; }

	/// <summary> Null if this mod doesn't own the given factory. </summary>
	string? KeyFor( IActionFactory factory );

	IActionFactory ResolveActionFactory( string key );
}
