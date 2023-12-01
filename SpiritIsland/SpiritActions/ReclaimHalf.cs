namespace SpiritIsland;

public class ReclaimHalf : SpiritAction {

	public ReclaimHalf():base( "Reclaim(1/2)" ) { }

	public override async Task ActAsync( Spirit self ) {
		int reclaimCount = (self.DiscardPile.Count + 1) / 2; // round up
		if(reclaimCount == 0) return;

		while(reclaimCount-- > 0)
			await self.Reclaim1FromDiscard();

	}

}