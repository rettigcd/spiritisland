using System.Threading.Tasks;

namespace SpiritIsland.PromoPack1 {

	public class GiftOfFlowingPower {

		[SpiritCard("Gift of Flowing Power",1,Element.Fire,Element.Water),Fast]
		[AnotherSpirit]
		public static Task ActAsync( TargetSpiritCtx ctx ) {
			// Target spirit gains 1 energy.
			ctx.Other.Energy += 1;

			// Target spirit chooses to either:
			return ctx.OtherCtx.SelectActionOption(
				// Play another Power Card by paying its cost
				new SelfAction("Play another Power Card by paying its cost", _ => { ctx.Other.AddActionFactory( new PlayCardForCost() ); } ), // !!!
				// OR Gains 1 fire and 1 water.
				new SelfAction("Gain 1 fire and 1 water", _ => { var els = ctx.Other.Elements; els[Element.Fire]++; els[Element.Water]++; } ) // !!!
			);
		}

	}

}
