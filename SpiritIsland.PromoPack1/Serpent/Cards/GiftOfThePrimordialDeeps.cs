using System.Threading.Tasks;

namespace SpiritIsland.PromoPack1 {

	public class GiftOfThePrimordialDeeps {

		[SpiritCard("Gift of the Primordial Deeps", 1, Element.Moon,Element.Earth), Fast, AnotherSpirit]
		public static async Task ActAsync( TargetSpiritCtx ctx ) {
			// target spirit gains a minor power.
			var powerCard = await ctx.OtherCtx.DrawMinor();

			// Target spirit chooses to either:
			await ctx.OtherCtx.SelectActionOption(
				new ActionOption(
					"Play it immediately by paying its cost", 
					() => { 
						ctx.Other.Energy -= powerCard.Cost;
						return powerCard.ActivateAsync( ctx.OtherCtx );
					},
					powerCard.Cost <= ctx.Other.Energy
				),
				new ActionOption(
					"Gains 1 moon and 1 earth", 
					() => { var els = ctx.Other.Elements; els[Element.Moon]++; els[Element.Earth]++; } 
				)
			);

		}
	}


}
