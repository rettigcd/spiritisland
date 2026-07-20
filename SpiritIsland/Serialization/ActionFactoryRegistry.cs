namespace SpiritIsland.Serialization;

/// <summary>
/// Resolves IActionFactory entries beyond PowerCard/InnatePower/GrowthAction/FastSlowAction (which
/// Spirit.SerializeActionFactory/DeserializeActionFactory handle directly) - same tag-dispatch shape as
/// BlightCardRegistry/SelfCmdRegistry. Types implement ISerializableActionFactory and self-register via
/// [ModuleInitializer]. Only the RepeatCardForCost family (RepeatCardForCost/RepeatCheapestCardForCost/
/// RepeatSpecificCardForCost) registers today - see docs/GameSerialization-Roadmap.md section 2/row 2.
/// </summary>
public static class ActionFactoryRegistry {

	public static IActionFactory Deserialize( JsonArray json, ISerializationContext ctx ) {
		string tag = json[0]!.GetValue<string>();
		return _readers.TryGetValue( tag, out var reader ) ? reader( json, ctx ) : throw new NotSupportedException( $"Unknown IActionFactory kind '{tag}'" );
	}

	public static void Register( string tag, Func<JsonArray, ISerializationContext, IActionFactory> reader ) => _readers[tag] = reader;

	static readonly Dictionary<string, Func<JsonArray, ISerializationContext, IActionFactory>> _readers = [];

}
