namespace SpiritIsland.Serialization;

/// <summary>
/// Resolves ISerializableSpiritMod entries captured by Spirit.ToJson/RestoreFromJson. Unlike most
/// registries here, a reader doesn't return a fresh instance - it applies JSON state directly onto
/// `spirit` (an Action, not a Func returning T), because most of these mods are already present in
/// spirit.Mods by the time this runs (added deterministically by the spirit's own constructor or its
/// aspect's ModSpirit, replayed for free before RestoreFromJson is ever called) and only need their
/// extra field(s) restored onto that same instance - constructing a new one and Mods.Add()-ing it would
/// create a duplicate. The few mods added dynamically mid-game (by a card/innate effect, to whichever
/// spirit it targets, rather than by spirit/aspect construction) aren't present yet, so their own reader
/// constructs one and Mods.Add()s it instead - same tag-dispatch shape as SelfCmdRegistry/
/// TimePassesActionRegistry either way.
/// </summary>
public static class SpiritModRegistry {

	public static void Restore( Spirit spirit, JsonArray json, ISerializationContext ctx ) {
		string tag = json[0]!.GetValue<string>();
		if( _readers.TryGetValue( tag, out var reader ) ) reader( spirit, json, ctx );
		else throw new NotSupportedException( $"Unknown ISerializableSpiritMod kind '{tag}'" );
	}

	public static void Register( string tag, Action<Spirit, JsonArray, ISerializationContext> reader ) => _readers[tag] = reader;

	static readonly Dictionary<string, Action<Spirit, JsonArray, ISerializationContext>> _readers = [];

}
