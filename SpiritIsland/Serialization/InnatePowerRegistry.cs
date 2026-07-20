namespace SpiritIsland.Serialization;

/// <summary>
/// Resolves an InnatePower by Title. Same reasoning as PowerCardRegistry: InnatePower has no per-instance
/// state worth capturing (LastTarget is transient scratch, not memento-captured either), so the same
/// registered instance is reused wherever referenced. Seeded from each GameComponentProvider.SpiritNames
/// list (constructing each spirit and reading its InnatePowers, the same way real game setup does) via
/// that provider's own [ModuleInitializer].
/// </summary>
public static class InnatePowerRegistry {

	public static InnatePower Deserialize( JsonArray json ) {
		string title = json[0]!.GetValue<string>();
		return _powers[title];
	}

	public static void Register( InnatePower power ) {
		if( _powers.TryGetValue( power.Title, out InnatePower? existing ) && existing.GeneralInstructions != power.GeneralInstructions )
			throw new InvalidOperationException( $"InnatePower collision: '{power.Title}' resolves to two different powers" );
		_powers[power.Title] = power;
	}

	static readonly Dictionary<string, InnatePower> _powers = [];

}
