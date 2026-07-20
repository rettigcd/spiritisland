
namespace SpiritIsland.Basegame;

/// <summary>
/// Resolve one slow, non-Major power during fast phase THIS ROUND ONLY
/// </summary>
/// <param name="spirit"></param>
class Run1SlowNonMajorAsFast(Spirit spirit) : RunSlowCardsAsFastMod_EveryRound(spirit), IEndWhenTimePasses, ISerializableSpiritMod {

	protected override int AllowedCount => 1;

	protected override bool EvaluateAction(IActionFactory action)
		=> base.EvaluateAction(action) && IsNonMajor(action);

	static bool IsNonMajor(IActionFactory slowAction) => slowAction is not PowerCard pc || pc.PowerType != PowerType.Major;

	#region Json

	// Short-lived (IEndWhenTimePasses) and added dynamically by ExaltationOfTheStormWind's tier-1 innate
	// to whichever spirit(s) it targets - not present after replaying spirit/aspect construction like
	// SwiftnessOfLightning is, so restoring this one constructs a fresh instance and Mods.Add()s it
	// rather than finding an existing one.
	const string Tag = "Run1SlowNonMajorAsFast";

	JsonArray ISerializableSpiritMod.ToJson( ISerializationContext ctx ) => new JsonArray( Tag, UsedCountForJson );

	[ModuleInitializer]
	internal static void RegisterSerialization()
		=> SpiritModRegistry.Register( Tag, ( spirit, json, ctx )
			=> spirit.Mods.Add( new Run1SlowNonMajorAsFast( spirit ) { UsedCountForJson = json[1]!.GetValue<int>() } ) );

	#endregion Json

}
