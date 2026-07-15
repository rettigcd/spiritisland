namespace SpiritIsland.Serialization;

/// <summary>
/// Resolves the "which command runs at the start of next round" identity for
/// GameCmd.AtTheStartOfNextRound entries (docs/GameSerialization-Roadmap.md section 10). Unlike a real
/// closure, every current caller (AllThingsWeaken, IntensifyingExploitation) builds a fully static,
/// parameterless IActOn&lt;GameState&gt; with no per-instance captured state - so all that needs to
/// round-trip is which named command it is, not any captured values. Same tag-dispatch shape as
/// BlightCardRegistry, seeded by each caller's own [ModuleInitializer].
/// </summary>
public static class NextRoundCommandRegistry {

	public static IActOn<GameState> Get( string tag ) => _factories[tag]();

	public static void Register( string tag, Func<IActOn<GameState>> factory ) => _factories[tag] = factory;

	static readonly Dictionary<string, Func<IActOn<GameState>>> _factories = [];

}
