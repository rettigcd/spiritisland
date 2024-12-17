namespace SpiritIsland.BranchAndClaw;

public class Encircle : IAspect {

	// https://spiritislandwiki.com/index.php?title=Encircle

	static public AspectConfigKey ConfigKey => new AspectConfigKey(SharpFangs.Name, Name);
	public const string Name = "Encircle";

	public string[] Replaces => [
		AllyOfTheBeasts.Name, // with Pack Hunting
		RangingHunt.Name, // with Encircle the Unsuspecting Prey
	];

	public void ModSpirit(Spirit spirit) {
		// Replace Ally with PackHunting
		spirit.RemovePresenceToken(); // Ally of the Beasts
		spirit.PowerRangeCalc = new PackHunting();
		spirit.SpecialRules[0] = PackHunting.Rule;

		// Replace Ranging Hunt with Encircle
		spirit.ReplaceInnate(RangingHunt.Name, InnatePower.For(typeof(EncircleTheUnsuspectingPrey)));
	}

}
