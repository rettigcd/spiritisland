namespace SpiritIsland.Basegame;

public class Reach : IAspect {

	// https://spiritislandwiki.com/index.php?title=Reach

	static public AspectConfigKey ConfigKey => new AspectConfigKey(Shadows.Name, Name);
	public const string Name = "Reach";
	public string[] Replaces => [ShadowsOfTheDahan.Name];

	public void ModSpirit(Spirit spirit) {
		ShadowsOfTheDahan.RemoveFrom(spirit);

		ReachThroughEphemeralDistance.InitAspect(spirit);

		spirit.SpecialRules = [ReachThroughEphemeralDistance.Rule];
	}

}
