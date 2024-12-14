namespace SpiritIsland.Basegame;

public class Haven : IAspect {

	// https://spiritislandwiki.com/index.php?title=Haven

	static public AspectConfigKey ConfigKey => new AspectConfigKey(RiverSurges.Name,Name);
	public const string Name = "Haven";
	public string[] Replaces => [MassiveFlooding.Name];

	public void ModSpirit(Spirit spirit) {

		CallToASunlitHaven.InitAspect(spirit);
	}
}
