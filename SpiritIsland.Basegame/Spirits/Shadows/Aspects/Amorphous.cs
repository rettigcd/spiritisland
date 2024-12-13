namespace SpiritIsland.Basegame;

public class Amorphous : IAspect {

	// https://spiritislandwiki.com/index.php?title=Amorphous

	// Special Rule Name   Shadows Partake of Amorphous Space
	// Special Rule Text   During each Fast phase, you may move 1 of your Presence to an adjacent land, or to a land with Dahan anywhere on the island.
	// During each Slow phase, you may move 1 of your Presence to an adjacent land, or to a land with Dahan anywhere on the island.

	static public AspectConfigKey ConfigKey => new AspectConfigKey(Shadows.Name, Name);
	public const string Name = "Amorphous";
	public string[] Replaces => [ShadowsOfTheDahan.Name];

	public void ModSpirit(Spirit spirit) {
	}

}
