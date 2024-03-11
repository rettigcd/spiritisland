namespace SpiritIsland;

public class PowerType : IOption {
	// None

	// PowerCards
	public static readonly PowerType Minor  = new PowerType("minor");
	public static readonly PowerType Major  = new PowerType("major");
	public static readonly PowerType Spirit = new PowerType("spirit");

	// Innate
	public static readonly PowerType Innate = new PowerType("innate");

	string IOption.Text => Name;
	public string Name { get; }

	PowerType(string text) { Name = text; }
}
