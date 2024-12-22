namespace SpiritIsland.FeatherAndFlame;

public class Transforming : IAspect {

	// https://spiritislandwiki.com/index.php?title=Transforming

	public const string Name = "Transforming";
	public static AspectConfigKey Key => new AspectConfigKey(HeartOfTheWildfire.Name, Name);
	public string[] Replaces => [TheBurnedLandRegrows.Name];

	public void ModSpirit(Spirit spirit) {
		// replace innate
		spirit.ReplaceInnate(TheBurnedLandRegrows.Name,InnatePower.For(typeof(ExaltationOfTheTransformingFlame)));

		// Implement TransformRatherThanConsume
		spirit.ReplacePresenceToken(new TransformRatherThanConsume(spirit));

		// Add Rule
		spirit.SpecialRules = [..spirit.SpecialRules,TransformRatherThanConsume.Rule];
	}

}