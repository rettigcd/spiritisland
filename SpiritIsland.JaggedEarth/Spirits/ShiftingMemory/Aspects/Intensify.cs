namespace SpiritIsland.JaggedEarth;

public class Intensify : IAspect {

	// https://spiritislandwiki.com/index.php?title=Intensify

	static public AspectConfigKey ConfigKey => new AspectConfigKey(ShiftingMemoryOfAges.Name, Name);
	public const string Name = "Intensify";

	public string[] Replaces => [InsightsIntoTheWorldsNature.Name];

	public void ModSpirit(Spirit spirit) {

		// !!! Bonus: Moon, Any

		IntensifyThroughUnderstanding.InitAspect(spirit);

		spirit.SpecialRules = [IntensifyThroughUnderstanding.Rule];
	}
}
