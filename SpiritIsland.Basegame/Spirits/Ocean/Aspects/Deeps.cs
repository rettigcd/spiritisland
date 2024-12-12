namespace SpiritIsland.Basegame;

public class Deeps : IAspect {

	// https://spiritislandwiki.com/index.php?title=Deeps

	static public AspectConfigKey ConfigKey => new AspectConfigKey(Ocean.Name, Name);
	public const string Name = "Deeps";
	public string[] Replaces => [OceanBreaksTheShore.Name];

	public void ModSpirit(Spirit spirit) {
		spirit.InnatePowers[0] = InnatePower.For(typeof(WaterEatsAwayTheDeepRootsOfEarth));
		spirit.InnatePowers[1] = InnatePower.For(typeof(ReclaimedByTheDeeps));
	}
}