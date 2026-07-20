namespace SpiritIsland.Serialization;

/// <summary>
/// Resolves an IAdversaryBuilder by name and builds an IAdversary from an AdversaryConfig. Same
/// "reuse the engine, seed via each expansion's own [ModuleInitializer]" approach as
/// PowerCardRegistry/InnatePowerRegistry - AdversaryBuilder instances are stateless/reusable
/// (IAdversaryBuilder.Build(level) just wraps `this` + level into a fresh Adversary, never mutates the
/// builder), so the same registered instance is handed back everywhere it's referenced. Only the
/// identity slice of docs/GameSerialization-Roadmap.md section 9 - doesn't touch Init/Adjust
/// wiring-replay, which is a separate, harder tier blocked on section 10.
/// </summary>
public static class AdversaryRegistry {

	public static IAdversary Build( AdversaryConfig cfg ) {
		IAdversaryBuilder builder = cfg.Name.Length == 0 ? new NullAdversaryBuilder() : _builders[cfg.Name];
		return builder.Build( cfg.Level );
	}

	public static void Register( IAdversaryBuilder builder ) => _builders[builder.Name] = builder;

	static readonly Dictionary<string, IAdversaryBuilder> _builders = [];

}
