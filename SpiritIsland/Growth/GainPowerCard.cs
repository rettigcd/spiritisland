namespace SpiritIsland;

public class GainPowerCard : SpiritAction {

	public GainPowerCard()
		:base(
			"Gain Power Card",
			ctx => ctx.Self.Draw()
		)
	{}
}