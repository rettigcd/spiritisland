namespace SpiritIsland;

public class ReclaimN( int count = 1 ) : SpiritAction( $"Reclaim({count})" ) {
	public override async Task ActAsync( Spirit self ) {
		if(self.DiscardPile.Count == 0) return;

		int reclaimCount = count;

		while(reclaimCount-- > 0)
			await self.Reclaim1FromDiscard();
	}

	public const string Prompt = "Select card to reclaim.";
}
