namespace SpiritIsland.Serialization;

/// <summary>
/// Resolves an IRunBeforeInvaderPhase entry by tag. Same shape as SpaceEntitySerialization/
/// BlightCardRegistry (tag-keyed reader dispatch, [ModuleInitializer] self-registration), kept
/// separate because IRunBeforeInvaderPhase is deliberately not ISpaceEntity (see the standing
/// correction in docs/ISpaceEntity-Types.md) - these live in GameState's own hook action list
/// (_preInvaderPhaseActions), not on a Space.
///
/// Only covers types whose IRunBeforeInvaderPhase entry is its own standalone object (like
/// SlowDissolutionOfWillMod). Several blight cards (DownwardSpiral, MemoryFadesToDust,
/// PowerCorrodesTheSpirit, UntendedLandCrumbles, AttenuatedEssence, BlightCorrodesTheSpirit) instead
/// register the BlightCard itself as the IRunBeforeInvaderPhase entry - reconstructing those means
/// resolving back to the existing GameState.BlightCard instance, not building a fresh object, which
/// this registry doesn't attempt. That's the harder half of
/// docs/GameSerialization-Roadmap.md section 10, still unsolved.
/// </summary>
public static class PreInvaderPhaseActionRegistry {

	public static IRunBeforeInvaderPhase Deserialize( JsonArray json, ISerializationContext ctx ) {
		string tag = json[0]!.GetValue<string>();
		return _readers[tag]( json, ctx );
	}

	public static void Register( string tag, Func<JsonArray, ISerializationContext, IRunBeforeInvaderPhase> reader ) => _readers[tag] = reader;

	static readonly Dictionary<string, Func<JsonArray, ISerializationContext, IRunBeforeInvaderPhase>> _readers = [];

}
