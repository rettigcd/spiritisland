namespace SpiritIsland.Basegame;

public class Amorphous : IAspect {

	// https://spiritislandwiki.com/index.php?title=Amorphous

	static public AspectConfigKey ConfigKey => new AspectConfigKey(Shadows.Name, Name);
	public const string Name = "Amorphous";
	public string[] Replaces => [ShadowsOfTheDahan.Name];

	public void ModSpirit(Spirit spirit) {
		ShadowsOfTheDahan.RemoveFrom(spirit);
		spirit.Mods.Add(new ShadowsPartakeOfAmorphousSpace(spirit));
	}

}
