namespace SpiritIsland.Basegame;

public class Foreboding : IAspect {

	// https://spiritislandwiki.com/index.php?title=Foreboding

	// Innate Name Stretch Out Coils of Foreboding Dread
	// Speed   Fast — Fast
	// Range	2
	// Target Any land

	// Innate Thresholds

	// 2 Air — 2 Air
	// Your other Powers may ignore Range when targeting the target land.

	// 1 Moon — 1 Moon
	// After an Action generates Fear in target land, including from Destroying Towns/Cities: Push up to 1 Explorer per Fear / 1 Town per 2 Fear. (You may mix-and-match.)

	// 2 Fire — 2 Fire
	// 1 Fear.

	// 2 Moon 4 Air — 2 Moon, 4 Air
	// 2 Fear.

	static public AspectConfigKey ConfigKey => new AspectConfigKey(Shadows.Name, Name);
	public const string Name = "Foreboding";
	public string[] Replaces => [Shadows.ShadowsOfTheDahan_Name];

	public void ModSpirit(Spirit spirit) {
	}

}
