namespace SpiritIsland.Basegame;

class FlourishWithNaturesStrength {

	public const string Name = "Flourish with Nature's Strength";
	const string Description = "After you gain a Major Power, gain a Minor Power.";
	static public SpecialRule Rule => new SpecialRule(Name, Description);

	static public void InitAspect(Spirit spirit) {
		// !!!
		throw new NotImplementedException();
	}

	// Flourish with Nature's Strength
	// After you gain a Major Power, gain a Minor Power.


}