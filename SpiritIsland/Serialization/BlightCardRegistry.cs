namespace SpiritIsland.Serialization;

/// <summary>
/// Resolves a BlightCard subclass by its type name. Same shape as SpaceEntitySerialization
/// (tag-keyed reader dispatch, taking both the json and the context so a reader can restore extra
/// state - e.g. SlowDissolutionOfWill's per-spirit token choice - not just call a parameterless
/// constructor), kept separate because BlightCard isn't ISpaceEntity - it's a GameState-level object,
/// like Island or Fear, not something placed on a Space. Types register themselves via
/// [ModuleInitializer] so core code never needs a hardcoded list of every blight card defined in
/// every expansion project.
/// </summary>
public static class BlightCardRegistry {

	public static BlightCard Deserialize( JsonArray json, ISerializationContext ctx ) {
		string name = json[0]!.GetValue<string>();
		BlightCard card = _readers[name]( json, ctx );
		card.CardFlipped = json[1]!.GetValue<bool>();
		return card;
	}

	public static void Register( string name, Func<JsonArray, ISerializationContext, BlightCard> reader ) => _readers[name] = reader;

	static readonly Dictionary<string, Func<JsonArray, ISerializationContext, BlightCard>> _readers = [];

}
