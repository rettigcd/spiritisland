namespace SpiritIsland.Serialization;

/// <summary>
/// Resolves an ITargetingSourceStrategy by tag - same tag-dispatch shape as BlightCardRegistry. Closes
/// the "known gap" flagged in docs/GameSerialization-Roadmap.md section 2: an *active* non-default
/// strategy (EntwinedPower's EntwinedPresenceSource, Locus of the Serpent's Regard's
/// IncludeSerpentsIncarna) used to throw in Spirit.ToJson/RestoreFromJson. Only 3 implementers exist
/// solution-wide, so a small closed registry (not a general Spirit.Mods mechanism) is enough.
/// </summary>
public static class TargetingSourceStrategyRegistry {

	public static ITargetingSourceStrategy Deserialize( JsonArray json, ISerializationContext ctx ) {
		string tag = json[0]!.GetValue<string>();
		return _readers[tag]( json, ctx );
	}

	public static void Register( string tag, Func<JsonArray, ISerializationContext, ITargetingSourceStrategy> reader ) => _readers[tag] = reader;

	static readonly Dictionary<string, Func<JsonArray, ISerializationContext, ITargetingSourceStrategy>> _readers = [];

}
