namespace SpiritIsland.Basegame;

public class Wind : IAspect {

	// https://spiritislandwiki.com/index.php?title=Wind

	static public AspectConfigKey ConfigKey => new AspectConfigKey(LightningsSwiftStrike.Name, Name);
	public const string Name = "Wind";
	public string[] Replaces => [SwiftnessOfLightning.Name]; // Exhaltation of StormWind

	public void ModSpirit(Spirit spirit) {
		// Replaces	Special Rule: Swiftness of Lightning
		spirit.RemoveRule(SwiftnessOfLightning.Name);
		spirit.RemoveMod<SwiftnessOfLightning>();

		spirit.AddInnate(InnatePower.For(typeof(ExaltationOfTheStormWind)));
	}
}
