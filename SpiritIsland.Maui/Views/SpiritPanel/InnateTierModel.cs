
namespace SpiritIsland.Maui;

public class InnateTierModel(IDrawableInnateTier tier,Spirit spirit) : ObservableModel {

	public ElementDictModel Elements { get; } = new ElementDictModel(tier.Elements);
	public string Description { get; } = tier.Description;
	public ECouldHaveElements HasElementsStatus { get => _hasElementsStatus; private set => SetProp(ref _hasElementsStatus, value); }

	internal void Update() {
		HasElementsStatus = spirit.Elements.CouldHave(tier.Elements);
	}

	// Color _bgColor = Colors.Transparent;
	ECouldHaveElements _hasElementsStatus;

}

public class InnateTierStatus(IDrawableInnateTier tier, Spirit spirit) {
	public ECouldHaveElements CouldActivateWith( CountDictionary<Element> cardElements) {
		return spirit.Elements.CouldHave(tier.Elements.Except(cardElements));
	}
}

public class InnateStatus(InnatePower power, Spirit spirit) {
	public int CountActivatedTiers( CountDictionary<Element> cardElements ) {
		return _tiers.Count(tier => tier.CouldActivateWith(cardElements) != ECouldHaveElements.No);
	}
	public string GetStatusString(CountDictionary<Element> cardElements) {
		int count = CountActivatedTiers(cardElements);
		return $"{_name}:{count}";
	}
	readonly string _name = power.Title.AbbreviateSentence();
	readonly InnateTierStatus[] _tiers = power.DrawableOptions.Select(t => new InnateTierStatus(t, spirit)).ToArray();
}