
namespace SpiritIsland.Basegame;

/// <summary>
/// Mod that inserts Slow cards into the ActionList during the Fast phase based on # of Air elements.
/// </summary>
sealed class SwiftnessOfLightning(Spirit spirit) : RunSlowCardsAsFastMod_EveryRound(spirit), ISerializableSpiritMod {

	public const string Name = "Swiftness of Lightning";
	const string Description = "For every Simple air you have, you may use 1 Slow Power as if it were fast";
	static public readonly SpecialRule Rule = new SpecialRule( Name, Description );

	protected override int AllowedCount => _spirit.Elements[Element.Air];

	#region Json

	// Always present for LightningsSwiftStrike unless the Wind aspect removed it (spirit's own
	// constructor, deterministic) - so this only ever needs to find-and-mutate the already-replayed
	// instance, never construct a new one.
	const string Tag = "SwiftnessOfLightning";

	JsonArray ISerializableSpiritMod.ToJson( ISerializationContext ctx ) => new JsonArray( Tag, UsedCountForJson );

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> SpiritModRegistry.Register( Tag, ( spirit, json, ctx )
			=> spirit.Mods.OfType<SwiftnessOfLightning>().Single().UsedCountForJson = json[1]!.GetValue<int>() );

	#endregion Json

}
