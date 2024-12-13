namespace SpiritIsland.Basegame;

public class Madness : IAspect {

	// https://spiritislandwiki.com/index.php?title=Madness

	// Special Rule
	// Shadows Cast a Subtle Madness
	// When you add Presence during Growth, you may also add 1 Strife in that land.

	// Special Rule
	// Glimpse of the Shadowed Void
	// When your Presence is Destroyed, if Invaders are present, 1 Fear per Presence Destroyed there.

	static public AspectConfigKey ConfigKey => new AspectConfigKey(Shadows.Name, Name);
	public const string Name = "Madness";
	public string[] Replaces => [ShadowsOfTheDahan.Name];

	public void ModSpirit(Spirit spirit) {
		spirit.RemoveMod<ShadowsOfTheDahan>();

	}

}
