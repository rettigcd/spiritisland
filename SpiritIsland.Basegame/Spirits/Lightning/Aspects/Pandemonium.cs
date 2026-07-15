namespace SpiritIsland.Basegame;

public class Pandemonium : IAspect {
	// https://spiritislandwiki.com/index.php?title=Pandemonium

	static public AspectConfigKey ConfigKey => new AspectConfigKey(LightningsSwiftStrike.Name, Name);
	public const string Name = "Pandemonium";
	public string[] Replaces => [ThunderingDestruction.Name]; // with Lightning-Torn Skies

	static InnatePower NewInnate => InnatePower.For(typeof(LightningTornSkiesIncitePandemonium));
	public InnatePower[] NewInnates => [NewInnate];

	public void ModSpirit(Spirit spirit) {
		spirit.ReplaceInnate(ThunderingDestruction.Name, NewInnate);
	}
}


