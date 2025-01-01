namespace SpiritIsland.Maui;

public class DeckModel(PowerType powerType, ImageSource source) : BaseOptionModel(powerType) {

	public ImageSource Image => source;

}
