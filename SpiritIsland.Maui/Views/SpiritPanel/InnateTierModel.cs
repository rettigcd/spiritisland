
namespace SpiritIsland.Maui;

public class InnateTierModel(IDrawableInnateTier tier,Spirit spirit) : ObservableModel {
	public ElementDictModel Elements { get; } = new ElementDictModel(tier.Elements);
	public string Description { get; } = tier.Description;

//	public bool IsActive { get => _isActive; private set { SetProp(ref _isActive,value); } }bool _isActive;
	public Color BgColor { get => _bgColor; private set => SetProp(ref _bgColor, value); } Color _bgColor = Colors.Transparent;

	internal void Update() {
		ECouldHaveElements result = spirit.CouldHaveElements(tier.Elements);
		BgColor = result switch {
			ECouldHaveElements.Yes => Colors.DarkSalmon,
			ECouldHaveElements.AsPrepared => Colors.Wheat,
			_ => Colors.Transparent
		};
	}
}