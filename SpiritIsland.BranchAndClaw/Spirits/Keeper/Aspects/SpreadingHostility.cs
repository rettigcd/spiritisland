namespace SpiritIsland.BranchAndClaw;

public class SpreadingHostility : IAspect {

	// https://spiritislandwiki.com/index.php?title=Spreading_Hostility

	public const string Name = "Spreading Hostility";
	static public AspectConfigKey ConfigKey => new AspectConfigKey(Keeper.Name,Name);
	public string[] Replaces => [];

	public void ModSpirit(Spirit spirit) {
		++spirit.Energy;

		// Rule 1 - Lands hostile to Humanity
		spirit.ReplacePresenceToken(new LandsHostileToHumanity(spirit));

		// Rule 2 - Energy Change
		foreach( Track t in spirit.Presence.Energy.Slots )
			if( t.Energy.HasValue ) {
				int newEnergy = (t.Energy.Value+1) /2;
				t.Energy = newEnergy;
				t.Code = Track.EnergyOnlyCode(newEnergy);
			}
		spirit.SpecialRules = [.. spirit.SpecialRules, ];

		// Rule 3 - Growth track
		var oldGroups = spirit.GrowthTrack.Groups;
		spirit.GrowthTrack = new(oldGroups[0], oldGroups[1], oldGroups[2], new GrowthGroup(
			new GainAllElements(Element.Fire),
			new MovePresence(1),
			new MovePresence(1),
			Cmd.DamageInvaders(1).In().SpiritPickedLand().Which(Has.YourPresence)
		));

		spirit.SpecialRules = [.. spirit.SpecialRules, LandsHostileToHumanity.Rule, SeedsOfTeaching_Rule, Growth_Rule];
	}

	static SpecialRule SeedsOfTeaching_Rule => new SpecialRule("Seeds of Teaching Sprout and Grow", "Energy numbers on your top Presence track are halved.");

	static SpecialRule Growth_Rule => new SpecialRule("Spreading Hostility", "Your rightmost (4th) Growth option is replaced with: Gain Fire — Move a Presence within 1 Range (x2) — 1 Damage at Range 0");
}
