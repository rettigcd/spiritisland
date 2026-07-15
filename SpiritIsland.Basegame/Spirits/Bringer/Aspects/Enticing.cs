namespace SpiritIsland.Basegame;

public class Enticing : IAspect {

	// https://spiritislandwiki.com/index.php?title=Enticing

	static public AspectConfigKey ConfigKey => new AspectConfigKey(Bringer.Name,Name);
	public const string Name = "Enticing";
	public string[] Replaces => [NightTerrors.Name];

	static InnatePower NewInnate => InnatePower.For(typeof(EnticingAndLullingDreams));
	public InnatePower[] NewInnates => [NewInnate];

	public void ModSpirit(Spirit spirit) {
		spirit.ReplaceInnate(NightTerrors.Name,NewInnate);
	}
}
