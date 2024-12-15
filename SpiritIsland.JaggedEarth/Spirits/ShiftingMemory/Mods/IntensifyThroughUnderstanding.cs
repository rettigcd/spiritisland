namespace SpiritIsland.JaggedEarth;

class IntensifyThroughUnderstanding {
	public const string Name = "Intensify Through Understanding";
	const string Description = "You may spend Element Markers to modify your Actions."
		+" • Sun: Add +1 Strife or +1 Badlands"
		+" • Moon: Remove/Replace +1 piece"
		+" • Fire: +1 Damage"
		+" • Air: Use Minor Power Fast"
		+" • Water: Move +1 piece"
		+" • Earth: Defend +2"
		+" • Plant: Add +2 Wilds or +2 Destroyed Presence"
		+" • Animal: Add +1 Disease or +1 Beasts";
	static public SpecialRule Rule => new SpecialRule(Name, Description);

	static public void InitAspect(Spirit spirit) {
		// !!! Implement Intensify
	}

	// You may spend Element Markers to modify your Actions (max. 1 of each Marker per Action)

	//Sun: Add +1 Strife or +1 Badlands
	//Moon: Remove/Replace +1 piece
	//Fire: +1 Damage
	//Air: Use Minor Power Fast
	//Water: Move +1 piece
	//Earth: Defend +2
	//Plant: Add +2 Wilds or +2 Destroyed Presence
	//Animal: Add +1 Disease or +1 Beasts
	// Except for Air Element, each modifier applies to 1 land. (The Action must already do that effect there.)

}
