namespace SpiritIsland.NatureIncarnate;

public class ReclaimAllWithFire : SpiritAction, ICanAutoRun {
	public ReclaimAllWithFire()
		: base(
			"Reclaim All with Fire",
			spirit => {
				static bool HasFire( PowerCard c ) => 0 < c.Elements[Element.Fire]; 
				spirit.Hand.AddRange( spirit.DiscardPile.Where( HasFire ) );
				spirit.DiscardPile.RemoveAll( HasFire );
			}
		) { }

}