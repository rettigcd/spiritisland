namespace SpiritIsland.Basegame;

public class Wind : IAspect {

	// https://spiritislandwiki.com/index.php?title=Wind

	static public AspectConfigKey ConfigKey => new AspectConfigKey(LightningsSwiftStrike.Name, Name);
	public const string Name = "Wind";
	public string[] Replaces => [SwiftnessOfLightning.Name];

	public void ModSpirit(Spirit spirit) {
		// Replaces	Special Rule: Swiftness of Lightning

		// remove old
		spirit.SpecialRules = spirit.SpecialRules.Where(x => x.Title != SwiftnessOfLightning.Name).ToArray();
		spirit.RemoveMod<SwiftnessOfLightning>();

		// add card
		spirit.InnatePowers = [.. spirit.InnatePowers, InnatePower.For(typeof(ExaltationOfTheStormWind))];
	}
}
