namespace SpiritIsland.Basegame;

public class Madness : IAspect {

	// https://spiritislandwiki.com/index.php?title=Madness

	// Special Rule 1 Name Shadows Cast a Subtle Madness
	// Special Rule 1 Text When you add Presence during Growth, you may also add 1 Strife in that land.

	// Special Rule 2 Name Glimpse of the Shadowed Void
	// Special Rule 2 Text When your Presence is Destroyed, if Invaders are present, 1 Fear per Presence Destroyed there.

	static public AspectConfigKey ConfigKey => new AspectConfigKey(Shadows.Name, Name);
	public const string Name = "Madness";
	public string[] Replaces => [Shadows.ShadowsOfTheDahan_Name];

	public void ModSpirit(Spirit spirit) {
	}

}
