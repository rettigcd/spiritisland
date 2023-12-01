namespace SpiritIsland;

public class ReclaimAll : SpiritAction, ICanAutoRun {
	public ReclaimAll()
		:base(
			"Reclaim All",
			ctx => {
				var spirit = ctx.Self;
				spirit.Hand.AddRange( spirit.DiscardPile );
				spirit.DiscardPile.Clear();
			}
		) { }
}