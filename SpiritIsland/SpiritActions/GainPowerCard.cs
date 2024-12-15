namespace SpiritIsland;

public class GainPowerCard : SpiritAction {

	public GainPowerCard()
		:base(
			"Gain Power Card",
			self => self.Draw.Card()
		)
	{}
}