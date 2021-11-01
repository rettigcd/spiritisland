using System.Threading.Tasks;

namespace SpiritIsland {

	public class DiscardElementsForCardPlay : GrowthActionFactory {

		readonly int totalNumToRemove;
		public DiscardElementsForCardPlay(int elementDiscardCount ) {
			this.totalNumToRemove = elementDiscardCount;
		}

		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			if( ctx.Self.PreparedElements.Total < totalNumToRemove ) return;

			var discarded = await ctx.Self.DiscardElements(totalNumToRemove);
			if(discarded.Total==totalNumToRemove)
				ctx.Self.tempCardPlayBoost++;
		}

	}
}
