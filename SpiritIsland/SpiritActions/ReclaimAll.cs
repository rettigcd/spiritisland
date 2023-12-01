namespace SpiritIsland;

public class ReclaimAll : SpiritAction, ICanAutoRun {
	public ReclaimAll()
		:base(
			"Reclaim All",
			self => {
				self.Hand.AddRange( self.DiscardPile );
				self.DiscardPile.Clear();
			}
		) { }
}