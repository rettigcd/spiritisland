namespace SpiritIsland.Basegame;

public class Tactician : IAspect {

	// TODO: add missing Growth image.

	// https://spiritislandwiki.com/index.php?title=Tactician

	static public AspectConfigKey ConfigKey => new AspectConfigKey(Thunderspeaker.Name, Name);
	public const string Name = "Tactician";
	public string[] Replaces => [/*Growth 3*/];

	public void ModSpirit(Spirit spirit) {
		spirit.SpecialRules = [..spirit.SpecialRules, Rule];

		// Remove 4th growth after it has been used
		var oldGroups = spirit.GrowthTrack.Groups;
		spirit.GrowthTrack = new(oldGroups[0],oldGroups[1], new GrowthGroup(
			new GainPowerCard(),
			new PlacePresence(0),
			new GainEnergy(1),
			new Gain1Element(Element.Sun, Element.Fire)
		));
	}

	static public SpecialRule Rule => new SpecialRule("Adaptable Tactician", "Your 3rd Growth option is replaced.");

}
