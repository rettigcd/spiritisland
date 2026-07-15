namespace SpiritIsland.Basegame;

public class Deeps : IAspect {

	// https://spiritislandwiki.com/index.php?title=Deeps

	static public AspectConfigKey ConfigKey => new AspectConfigKey(Ocean.Name, Name);
	public const string Name = "Deeps";
	public string[] Replaces => [OceanBreaksTheShore.Name];

	static InnatePower NewInnate0 => InnatePower.For(typeof(WaterEatsAwayTheDeepRootsOfEarth));
	static InnatePower NewInnate1 => InnatePower.For(typeof(ReclaimedByTheDeeps));
	public InnatePower[] NewInnates => [NewInnate0, NewInnate1];

	public void ModSpirit(Spirit spirit) {
		spirit.InnatePowers[0] = NewInnate0;
		spirit.InnatePowers[1] = NewInnate1;
	}
}