namespace SpiritIsland.Serialization;

/// <summary>
/// Resolves IRunWhenTimePasses entries. Tokens_ForIsland/Healer/Spirit are well-known per-game
/// singletons - resolution goes through ISerializationContext (which already knows the live
/// GameState's Tokens/Healer/Spirits) rather than a registry lookup, since there's nothing to
/// register, every game has exactly one of each.
///
/// Everything else registers itself by tag via [ModuleInitializer], same tag-dispatch shape as
/// BlightCardRegistry/PreInvaderPhaseActionRegistry - types implement ISerializableTimePassesAction
/// (not a plain interface member on IRunWhenTimePasses itself, since Tokens_ForIsland and Spirit each
/// already have an unrelated ToJson(ISerializationContext) for a completely different purpose - full
/// board/spirit state - and reusing that signature here would be a confusing double meaning).
///
/// Remaining gaps (card/blight-effect-created objects needing identity resolution back to an
/// already-placed live instance, and the 8 TimePassesAction.Once(...) closures) are documented in
/// docs/GameSerialization-Roadmap.md section 10, not handled here - Serialize/Deserialize throw
/// NotSupportedException for anything not registered, same fail-loud approach as every other registry
/// in this project.
/// </summary>
public static class TimePassesActionRegistry {

	public static JsonArray Serialize( IRunWhenTimePasses action, ISerializationContext ctx ) => action switch {
		Tokens_ForIsland => new JsonArray( "Tokens" ),
		Healer healer => new JsonArray( "Healer", healer.ToJson() ),
		Spirit spirit => new JsonArray( "Spirit", ctx.IndexOf( spirit ) ),
		ISerializableTimePassesAction custom => custom.ToJson( ctx ),
		_ => throw new NotSupportedException( $"TimePassesActionRegistry doesn't know how to serialize IRunWhenTimePasses of type {action.GetType().Name} yet - implement ISerializableTimePassesAction and register a reader (see docs/GameSerialization-Roadmap.md section 10)." )
	};

	public static IRunWhenTimePasses Deserialize( JsonArray json, ISerializationContext ctx ) {
		string tag = json[0]!.GetValue<string>();
		return tag switch {
			"Tokens" => ctx.Tokens,
			"Healer" => RestoreHealer( ctx, (JsonArray)json[1]! ),
			"Spirit" => ctx.SpiritAt( json[1]!.GetValue<int>() ),
			_ => _readers.TryGetValue( tag, out var reader ) ? reader( json, ctx ) : throw new NotSupportedException( $"Unknown IRunWhenTimePasses kind '{tag}'" )
		};
	}

	static Healer RestoreHealer( ISerializationContext ctx, JsonArray json ) {
		ctx.Healer.RestoreFromJson( json, ctx );
		return ctx.Healer;
	}

	public static void Register( string tag, Func<JsonArray, ISerializationContext, IRunWhenTimePasses> reader ) => _readers[tag] = reader;

	static readonly Dictionary<string, Func<JsonArray, ISerializationContext, IRunWhenTimePasses>> _readers = [];

}
