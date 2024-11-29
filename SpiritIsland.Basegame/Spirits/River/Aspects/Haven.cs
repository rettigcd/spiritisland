namespace SpiritIsland.Basegame;

public class Haven : IAspect {

	static public AspectConfigKey ConfigKey => new AspectConfigKey(RiverSurges.Name,Name);

	public const string Name = "Haven";

	public void ModSpirit(Spirit spirit) {
		// Replace Innate
		spirit.InnatePowers[0] = InnatePower.For(typeof(CallToASunlitHaven));
	}
}
