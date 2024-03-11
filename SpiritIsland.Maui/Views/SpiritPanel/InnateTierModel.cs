namespace SpiritIsland.Maui;

public class InnateTierModel(IDrawableInnateTier tier) : ObservableModel {
	public ElementDictModel Elements { get; } = new ElementDictModel(tier.Elements);
	public string Description { get; } = tier.Description;
}