namespace SpiritIsland.Basegame;

public class Reach : IAspect {

	// https://spiritislandwiki.com/index.php?title=Reach

	// Special Rule Name   Reach Through Ephemeral Distance
	// Special Rule Text Once per turn, you may ignore Range.
	// (This can be during Growth or for a Power - anything for which there's a Range arrow or the word "Range" is used. It affects a single Action of yours.)

	static public AspectConfigKey ConfigKey => new AspectConfigKey(Shadows.Name, Name);
	public const string Name = "Reach";
	public string[] Replaces => [ShadowsOfTheDahan.Name];

	public void ModSpirit(Spirit spirit) {
	}

}
