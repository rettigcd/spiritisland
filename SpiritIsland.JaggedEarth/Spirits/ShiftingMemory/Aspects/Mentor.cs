namespace SpiritIsland.JaggedEarth;

public class Mentor : IAspect {

	// https://spiritislandwiki.com/index.php?title=Mentor

	static public AspectConfigKey ConfigKey => new AspectConfigKey(ShiftingMemoryOfAges.Name, Name);
	public const string Name = "Mentor";

	public string[] Replaces => [LongAgesOfKnowledgeAndForgetfulness.Name];

	public void ModSpirit(Spirit spirit) {

		spirit.ReplaceInnate( ObserveTheEverChangingWorld.Name, InnatePower.For(typeof(ShareMentorshipAndExpertise)));
		PastTeachingsSpringForthUnbidden.InitAspect(spirit);

		spirit.SpecialRules = [PastTeachingsSpringForthUnbidden.Rule];
	}
}