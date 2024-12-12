namespace SpiritIsland.Basegame;

public class Wind : IAspect {

	// https://spiritislandwiki.com/index.php?title=Wind

	static public AspectConfigKey ConfigKey => new AspectConfigKey(LightningsSwiftStrike.Name, Name);
	public const string Name = "Wind";

	public void ModSpirit(Spirit spirit) {
		// Replaces	Special Rule: Swiftness of Lightning

		// remove old
		var swiftnessOfLightingTitle = SwiftnessOfLightning.Rule.Title;
		spirit.SpecialRules = spirit.SpecialRules.Where(x => x.Title != swiftnessOfLightingTitle).ToArray();
		var swiftnessOfLightingMod = spirit.Mods.OfType<SwiftnessOfLightning>().Single();
		spirit.Mods.Remove(swiftnessOfLightingMod);

		// add card
		spirit.InnatePowers = [.. spirit.InnatePowers, InnatePower.For(typeof(ExaltationOfTheStormWind))];
	}
}
