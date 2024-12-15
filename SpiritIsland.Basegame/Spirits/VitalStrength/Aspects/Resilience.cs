namespace SpiritIsland.Basegame;

public class Resilience : IAspect {

	// https://spiritislandwiki.com/index.php?title=Resilience

	static public AspectConfigKey ConfigKey => new AspectConfigKey(VitalStrength.Name, Name);
	public const string Name = "Resilience";
	public string[] Replaces => [EarthsVitality.Name];

	public void ModSpirit(Spirit spirit) {

		EarthsVitality.ReplaceWith(spirit, new AnchorTheLandsResilience(spirit));

		spirit.SpecialRules = [AnchorTheLandsResilience.Rule];
	}

}
