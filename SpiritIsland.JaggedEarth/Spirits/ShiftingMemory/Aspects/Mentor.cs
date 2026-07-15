namespace SpiritIsland.JaggedEarth;

public class Mentor : IAspect {

	// https://spiritislandwiki.com/index.php?title=Mentor

	static public AspectConfigKey ConfigKey => new AspectConfigKey(ShiftingMemoryOfAges.Name, Name);
	public const string Name = "Mentor";

	public string[] Replaces => [LongAgesOfKnowledgeAndForgetfulness.Name];

	static InnatePower NewInnate => InnatePower.For(typeof(ShareMentorshipAndExpertise));
	public InnatePower[] NewInnates => [NewInnate];

	public void ModSpirit(Spirit spirit) {

		spirit.ReplaceInnate( ObserveTheEverChangingWorld.Name, NewInnate);
		PastTeachingsSpringForthUnbidden.InitAspect(spirit);

		spirit.SpecialRules = [PastTeachingsSpringForthUnbidden.Rule];
	}
}