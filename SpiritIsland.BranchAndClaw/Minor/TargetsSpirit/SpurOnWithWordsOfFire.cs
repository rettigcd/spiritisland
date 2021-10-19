using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class SpurOnWithWordsOfFire {

		[MinorCard("Spur on with Words of Fire", 1, Element.Sun, Element.Fire, Element.Air)]
		[Fast]
		[AnySpirit]
		static public async Task ActAsync(TargetSpiritCtx ctx) {

			// If you target a spirit other than yourself, they gain +1 energy
			if(ctx.Other != ctx.Self)
				ctx.Other.Energy++;

			// target spirit may immediately play another power Card by paying its cost.
			// if it is slow, it does not resolve until later
			await ctx.Other.PurchaseCardsFromHand(1);

		}

	}

}
