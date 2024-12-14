namespace SpiritIsland.Basegame;

public class Travel : IAspect {

	// https://spiritislandwiki.com/index.php?title=Travel

	static public AspectConfigKey ConfigKey => new AspectConfigKey(RiverSurges.Name, Name);
	public const string Name = "Travel";
	public string[] Replaces => [RiversDomain.Name];

	public void ModSpirit(Spirit spirit) {
		if( spirit is not RiverSurges) throw new ArgumentException("Travel may only be applied to River.");
		// Replace 'River's Domain' with Travel and Tend
		spirit.SpecialRules = [ TravelOnRiversBack.Rule, TendingPresence.Rule];

		// Add Post-Growth Action (after precenses are placed).
		var old = spirit.Presence;
		old.Energy.Slots.First().Action = new TravelOnRiversBack();

		// Replace Presene and Token
		spirit.Presence = new TendingPresence(spirit, old.Energy, old.CardPlays);

	}

}
