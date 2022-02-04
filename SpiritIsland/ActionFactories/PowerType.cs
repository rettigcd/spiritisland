namespace SpiritIsland;

public class PowerType : IOption {
	public static readonly PowerType Minor  = new PowerType("minor");
	public static readonly PowerType Major  = new PowerType("major");
	public static readonly PowerType Spirit = new PowerType("spirit");
	public static readonly PowerType Innate = new PowerType("innate");

	public string Text { get; }

	PowerType(string text) { Text = text; }
}
