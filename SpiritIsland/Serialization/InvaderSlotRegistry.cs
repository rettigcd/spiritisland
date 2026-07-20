namespace SpiritIsland.Serialization;

/// <summary>
/// Resolves an "extra" InvaderSlot (anything beyond InvaderDeck's fixed Explore/Build/Ravage
/// instances - e.g. England's HighImmegrationSlot) by its type tag. Same tag-dispatch shape as
/// BlightCardRegistry/SpaceEntitySerialization - types register themselves via [ModuleInitializer]
/// so core code never needs to know about adversary-specific slot types.
/// </summary>
public static class InvaderSlotRegistry {

	public static InvaderSlot Deserialize( JsonArray json ) => _readers[json[0]!.GetValue<string>()]( json );

	public static void Register( string tag, Func<JsonArray, InvaderSlot> reader ) => _readers[tag] = reader;

	static readonly Dictionary<string, Func<JsonArray, InvaderSlot>> _readers = [];

}
