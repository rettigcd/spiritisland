namespace SpiritIsland.Basegame;

public class Foreboding : IAspect {

	// https://spiritislandwiki.com/index.php?title=Foreboding

	static public AspectConfigKey ConfigKey => new AspectConfigKey(Shadows.Name, Name);
	public const string Name = "Foreboding";
	public string[] Replaces => [ShadowsOfTheDahan.Name];

	public void ModSpirit(Spirit spirit) {
		spirit.RemoveMod<ShadowsOfTheDahan>();
		spirit.InnatePowers = [..spirit.InnatePowers, InnatePower.For(typeof(StretchOutCoilsOfForebodingDread))];
	}

}
