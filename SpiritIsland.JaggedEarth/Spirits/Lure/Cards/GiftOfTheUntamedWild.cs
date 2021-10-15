using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth.Spirits.Lure {
	public class GiftOfTheUntamedWild {

		[SpiritCard("Gift of the Untamed Wild",0,Element.Moon,Element.Fire,Element.Air,Element.Plant),Slow,AnySpirit]
		static public Task ActAsync(TargetSpiritCtx ctx ) {

			// target spirit chooses to either: 
			return ctx.OtherCtx.SelectActionOption(
				// Add 1 wilds to one of their lands
				new ActionOption("Add 1 wilds to one of your lands", () => Add1WildsToOneOfYourLands(ctx.OtherCtx)),
				// Replace 1 of their presence with 1 disease.
				new ActionOption("Replace 1 of your presence with 1 disease", () => Replace1PresenceWith1Disease(ctx.OtherCtx))
			);
		}

		static async Task Add1WildsToOneOfYourLands(SpiritGameStateCtx ctx) {
			var spaceCtx = await ctx.SelectSpace("Add 1 Wilds",ctx.Self.Presence.Spaces);
			spaceCtx.Wilds.Count++;
		}

		static async Task Replace1PresenceWith1Disease(SpiritGameStateCtx ctx) {
			var space = await ctx.SelectDeployedPresence("Replace Presence with 1 disease");
			ctx.Self.Presence.RemoveFrom(space);
			ctx.Target(space).Disease.Count++;
		}


	}


}
