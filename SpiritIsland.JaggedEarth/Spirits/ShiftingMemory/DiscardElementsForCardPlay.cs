using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	/// <summary>
	/// Shifting Memories Track-Action
	/// </summary>
	public class DiscardElementsForCardPlay : GrowthActionFactory {

		readonly int totalNumToRemove;
		public DiscardElementsForCardPlay(int elementDiscardCount ) {
			this.totalNumToRemove = elementDiscardCount;
		}

		public override async Task ActivateAsync( SpiritGameStateCtx ctx ) {
			if( ctx.Self is ShiftingMemoryOfAges smoa
				&& totalNumToRemove <= smoa.PreparedElements.Total
				&& (await smoa.DiscardElements(totalNumToRemove)).Count == totalNumToRemove
			) smoa.tempCardPlayBoost++;
		}

	}
}
