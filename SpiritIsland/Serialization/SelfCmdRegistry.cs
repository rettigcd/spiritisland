namespace SpiritIsland.Serialization;

/// <summary>
/// Resolves IActOn&lt;Spirit&gt; commands wrapped by either FastSlowAction, or a GrowthAction built via
/// `.ToGrowth()` and added directly via AddActionFactory outside GrowthTrack (see
/// Spirit.SerializeGrowthAction) - same tag-dispatch shape as BlightCardRegistry/TimePassesActionRegistry.
/// Types implement ISerializableSelfCmd and self-register via [ModuleInitializer]. Registered today:
/// PlayCardForCost (wrapped by FastSlowAction), and 3 one-off SpiritAction subclasses wrapped by
/// `.ToGrowth()` - Locus.PlaceIncarnaAndFireEnergy, WarriorSpiritsRaidingParty.PlaceIncarna,
/// Lair.InitLair.
/// </summary>
public static class SelfCmdRegistry {

	public static JsonArray Serialize( IActOn<Spirit> cmd, ISerializationContext ctx ) => cmd switch {
		ISerializableSelfCmd custom => custom.ToJson( ctx ),
		_ => throw new NotSupportedException( $"SelfCmdRegistry doesn't know how to serialize IActOn<Spirit> of type {cmd.GetType().Name} yet - implement ISerializableSelfCmd and register a reader." )
	};

	public static IActOn<Spirit> Deserialize( JsonArray json, ISerializationContext ctx ) {
		string tag = json[0]!.GetValue<string>();
		return _readers.TryGetValue( tag, out var reader ) ? reader( json, ctx ) : throw new NotSupportedException( $"Unknown IActOn<Spirit> kind '{tag}'" );
	}

	public static void Register( string tag, Func<JsonArray, ISerializationContext, IActOn<Spirit>> reader ) => _readers[tag] = reader;

	static readonly Dictionary<string, Func<JsonArray, ISerializationContext, IActOn<Spirit>>> _readers = [];

}
