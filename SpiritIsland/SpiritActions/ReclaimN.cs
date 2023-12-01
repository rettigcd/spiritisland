namespace SpiritIsland;

public class ReclaimN : SpiritAction {

	public ReclaimN( int count = 1 ):base( $"Reclaim({count})" ) {
		_count = count;
	}

	public override async Task ActAsync( Spirit self ) {
		if(self.DiscardPile.Count == 0) return;

		int reclaimCount = _count;

		while(reclaimCount-- > 0)
			await self.Reclaim1FromDiscard();
	}

	public const string Prompt = "Select card to reclaim.";

	readonly int _count;

}
