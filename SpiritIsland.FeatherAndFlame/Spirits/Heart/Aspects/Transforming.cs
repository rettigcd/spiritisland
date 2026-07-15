namespace SpiritIsland.FeatherAndFlame;

public class Transforming : IAspect {

	// https://spiritislandwiki.com/index.php?title=Transforming

	public const string Name = "Transforming";
	public static AspectConfigKey Key => new AspectConfigKey(HeartOfTheWildfire.Name, Name);
	public string[] Replaces => [TheBurnedLandRegrows.Name];

	static InnatePower NewInnate => InnatePower.For(typeof(ExaltationOfTheTransformingFlame));
	public InnatePower[] NewInnates => [NewInnate];

	public void ModSpirit(Spirit spirit) {
		// replace innate
		spirit.ReplaceInnate(TheBurnedLandRegrows.Name,NewInnate);

		// Implement TransformRatherThanConsume
		spirit.ReplacePresenceToken(new TransformRatherThanConsume(spirit));

		// Add Rule
		spirit.SpecialRules = [..spirit.SpecialRules,TransformRatherThanConsume.Rule];
	}

}