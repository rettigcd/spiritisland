namespace SpiritIsland;

public class ReclaimN : SpiritAction {

	public ReclaimN( int count = 1 ):base( $"Reclaim({count})" ) {
		_count = count;
	}

	public override async Task ActAsync( SelfCtx ctx ) {
		if(ctx.Self.DiscardPile.Count == 0) return;

		int reclaimCount = _count;

		while(reclaimCount-- > 0)
			await ctx.Self.Reclaim1FromDiscard();
	}

	public const string Prompt = "Select card to reclaim.";

	readonly int _count;

}


public class ReclaimHalf : SpiritAction {

	public ReclaimHalf():base( "Reclaim(1/2)" ) { }

	public override async Task ActAsync( SelfCtx ctx ) {
		int reclaimCount = (ctx.Self.DiscardPile.Count + 1) / 2; // round up
		if(reclaimCount == 0) return;

		while(reclaimCount-- > 0)
			await ctx.Self.Reclaim1FromDiscard();

	}

}