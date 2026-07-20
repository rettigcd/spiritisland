namespace SpiritIsland;

/// <summary>
/// Implemented by `InvaderSlot` subclasses that can appear in `InvaderDeck.ActiveSlots` beyond the
/// three fixed, always-present slots (`Explore`/`Build`/`Ravage`) - e.g. England's
/// `HighImmegrationSlot`. `InvaderDeck.ToJson`/`FromJson` resolves the three fixed slots by label
/// against the deck's own instances (they always exist); anything else must implement this and
/// register a reader in `InvaderSlotRegistry`, the same tag-dispatch shape `BlightCardRegistry`/
/// `SpaceEntitySerialization` use. The first element of the returned array is always the type-tag
/// used to find the matching reader.
/// </summary>
public interface ISerializableInvaderSlot {
	JsonArray ToJson();
}
