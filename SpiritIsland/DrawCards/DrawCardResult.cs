namespace SpiritIsland;

public class DrawCardResult( PowerType _powerType ) {
	public PowerType PowerType { get; } = _powerType;
	public required PowerCard[] SelectedCards;
	public PowerCard Selected => SelectedCards.Single();
	public List<PowerCard>? Rejected;
}