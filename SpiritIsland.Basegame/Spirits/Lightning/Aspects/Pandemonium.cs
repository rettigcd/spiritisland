namespace SpiritIsland.Basegame;

public class Pandemonium : IAspect {
	// https://spiritislandwiki.com/index.php?title=Pandemonium

	static public AspectConfigKey ConfigKey => new AspectConfigKey(LightningsSwiftStrike.Name, Name);
	public const string Name = "Pandemonium";

	public void ModSpirit(Spirit spirit) {
		// Replaces Innate Power: Thundering Destruction
		spirit.InnatePowers[0] = InnatePower.For(typeof(LightningTornSkiesIncitePandemonium));
	}
}


