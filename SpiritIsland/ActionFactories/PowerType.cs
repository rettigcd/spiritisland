namespace SpiritIsland;

public class PowerType : IOption {
	// None

	// PowerCards
	public static readonly PowerType Minor  = new PowerType("minor");
	public static readonly PowerType Major  = new PowerType("major");
	public static readonly PowerType Spirit = new PowerType("spirit");

	// Innate
	public static readonly PowerType Innate = new PowerType("innate");

	public string Text { get; }

	PowerType(string text) { Text = text; }
}

// Volcano targets differently for Innates vs cards
// !!! Instead of this, use PowerType above for Volcano targetting
public enum TargetingPowerType { 
	None, 
	Innate, 
	PowerCard
} // Can't think up a good name for this