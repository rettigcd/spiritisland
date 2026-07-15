namespace SpiritIsland.BranchAndClaw;

public class Unconstrained : IAspect {

	// https://spiritislandwiki.com/index.php?title=Unconstrained

	static public AspectConfigKey ConfigKey => new AspectConfigKey(SharpFangs.Name, Name);
	public const string Name = "Unconstrained";

	public string[] Replaces => [CallForthPredators.Name];

	static InnatePower NewInnate => InnatePower.For(typeof(RangingHuntOnBlight));
	public InnatePower[] NewInnates => [NewInnate];

	public void ModSpirit(Spirit spirit) {
		// Upgrade Ranging Hunt
		spirit.ReplaceInnate(RangingHunt.Name, NewInnate);

		// !!! Do not Add a Beasts as part of Spirit Setup.
		((SharpFangs)spirit).SetupAction = null;

		// Replace "Call Forth Preditors" with "Predattors Arise"
		spirit.GrowthTrack.PickGroups.Last().Groups[0].Actions[0] 
			= new PredatorsArise();

		spirit.ReplaceRule(CallForthPredators.Name, PredatorsArise.Rule);
	}

}

