namespace SpiritIsland.Basegame;

public class Deeps : IAspect {

	static public AspectConfigKey ConfigKey => new AspectConfigKey(Ocean.Name, Name);

	public const string Name = "Deeps";

	public void ModSpirit(Spirit spirit) {
		spirit.InnatePowers[0] = InnatePower.For(typeof(WaterEatsAwayTheDeepRootsOfEarth));
		spirit.InnatePowers[1] = InnatePower.For(typeof(ReclaimedByTheDeeps));
	}
}