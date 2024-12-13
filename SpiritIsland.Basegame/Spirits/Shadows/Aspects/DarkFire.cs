namespace SpiritIsland.Basegame;

public class DarkFire : IAspect {

	// https://spiritislandwiki.com/index.php?title=Dark_Fire

	// Setup Gain Unquenchable Flames(Minor Power)

	// Special Rule 1 Name Dark and Fire as One
	// Special Rule 1 Text You may treat each Moon available to you as being Fire, or vice versa. (Choose during each Action for each Moon/Fire you have.) You may discard or Forget Powers that grant Moon to pay for Fire Choice Events, and vice versa.

	// Special Rule 2 Name Frightful Shadows Elude Destruction
	// Special Rule 2 Text The first time each Action would destroy your Presence, you may Push 1 of those Presence instead of destroying it.

	// Bonus Presence Track Icon   Fire(or Moon)

	static public AspectConfigKey ConfigKey => new AspectConfigKey(Shadows.Name, Name);
	public const string Name = "Dark Fire";
	public string[] Replaces => [ShadowsOfTheDahan.Name];

	public void ModSpirit(Spirit spirit) {
	}

}
