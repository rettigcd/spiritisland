

namespace SpiritIsland.Basegame;

public class Madness : IAspect {

	// https://spiritislandwiki.com/index.php?title=Madness

	static public AspectConfigKey ConfigKey => new AspectConfigKey(Shadows.Name, Name);
	public const string Name = "Madness";
	public string[] Replaces => [ShadowsOfTheDahan.Name];

	public void ModSpirit(Spirit spirit) {
		ShadowsOfTheDahan.RemoveFrom(spirit);

		// Rule 1
		ShadowsCastASubtleMadness_InitAspect(spirit);

		// Rule 2
		GlimpsOfTheShadowedVoid.InitAspect(spirit);

		spirit.SpecialRules = [SubtleMadnessRule, GlimpsOfTheShadowedVoid.Rule];
	}

	#region Shadows Cast a SubtleMadness

	static void ShadowsCastASubtleMadness_InitAspect(Spirit spirit) {
		foreach( var pp in spirit.GrowthTrack.GrowthActions.OfType<PlacePresence>() )
			pp.Placed.Add(args => spirit.Target((Space)args.To).AddStrife(1));
	}

	static SpecialRule SubtleMadnessRule => new SpecialRule(
		"Shadows Cast a Subtle Madness",
		"When you add Presence during Growth, you may also add 1 Strife in that land."
	);
	#endregion Shadows Cast a SubtleMadness

}
