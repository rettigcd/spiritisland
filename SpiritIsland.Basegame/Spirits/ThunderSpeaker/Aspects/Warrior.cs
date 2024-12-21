namespace SpiritIsland.Basegame;

public class Warrior : IAspect {

	// TODO:
	// Correct Incarna Image
	// Inital setup should not be optional.
	// Share Moving with regular presence.

	// https://spiritislandwiki.com/index.php?title=Warrior

	static public AspectConfigKey ConfigKey => new AspectConfigKey(Thunderspeaker.Name, Name);
	public const string Name = "Warrior";
	public string[] Replaces => [LeadTheFuriousAssult.Name,ManifestationOfPowerAndGlory.Name];

	public void ModSpirit(Spirit spirit) {
		spirit.ReplaceInnate(LeadTheFuriousAssult.Name,InnatePower.For(typeof(LeadTheWarriorsToBattle)));
		spirit.ReplaceCard(ManifestationOfPowerAndGlory.Name,PowerCard.ForDecorated(CallToBloodshed.ActAsync));
		spirit.ReplaceIncarna(new WarriorSpiritsRaidingParty(spirit));
		spirit.AddActionFactory(new SpiritAction("Place Incarna", WarriorSpiritsRaidingParty.PlaceIncarna).ToGrowth());
		spirit.SpecialRules = [..spirit.SpecialRules, WarriorSpiritsRaidingParty.Rule];
	}
}
