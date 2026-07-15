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

	static InnatePower NewInnate => InnatePower.For(typeof(LeadTheWarriorsToBattle));
	public InnatePower[] NewInnates => [NewInnate];
	static PowerCard NewCard => PowerCard.ForDecorated(CallToBloodshed.ActAsync);
	public PowerCard[] NewCards => [NewCard];

	public void ModSpirit(Spirit spirit) {
		spirit.ReplaceInnate(LeadTheFuriousAssult.Name,NewInnate);
		spirit.ReplaceCard(ManifestationOfPowerAndGlory.Name,NewCard);
		spirit.ReplaceIncarna(new WarriorSpiritsRaidingParty(spirit));
		spirit.AddActionFactory(new WarriorSpiritsRaidingParty.PlaceIncarna().ToGrowth());
		spirit.SpecialRules = [..spirit.SpecialRules, WarriorSpiritsRaidingParty.Rule];
	}
}
