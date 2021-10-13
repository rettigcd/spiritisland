using System.Threading.Tasks;

namespace SpiritIsland.PromoPack1 {

	public class GiftOfFlowingPower {

		[SpiritCard("Gift of Flowing Power",1,Element.Fire,Element.Water),Fast]
		[TargetSpirit]
		public static Task ActAsync(TargetSpiritCtx ctx ) {
			// Target spirit gains 1 energy.
			// Target spirit chooses to either:
				// Play another Power Card by paying its cost
				// OR Gains 1 fire and 1 water.

			return Task.CompletedTask;
		}
	}

}
