namespace SpiritIsland.Serialization;

/// <summary>
/// Resolves an IRunAfterInvaderPhase entry by tag. Same shape as PreInvaderPhaseActionRegistry (tag-keyed
/// reader dispatch, [ModuleInitializer] self-registration) - kept separate because it's a different hook
/// list (GameState._postInvaderPhaseActions), not interchangeable with _preInvaderPhaseActions/
/// _timePassesActions. See docs/GameSerialization-Roadmap.md section 10.
/// </summary>
public static class PostInvaderPhaseActionRegistry {

	public static IRunAfterInvaderPhase Deserialize( JsonArray json, ISerializationContext ctx ) {
		string tag = json[0]!.GetValue<string>();
		return _readers[tag]( json, ctx );
	}

	public static void Register( string tag, Func<JsonArray, ISerializationContext, IRunAfterInvaderPhase> reader ) => _readers[tag] = reader;

	static readonly Dictionary<string, Func<JsonArray, ISerializationContext, IRunAfterInvaderPhase>> _readers = [];

}
