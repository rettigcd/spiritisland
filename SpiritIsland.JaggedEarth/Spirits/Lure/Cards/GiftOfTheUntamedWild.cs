using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class GiftOfTheUntamedWild {

		[SpiritCard("Gift of the Untamed Wild",0,Element.Moon,Element.Fire,Element.Air,Element.Plant),Slow,AnySpirit]
		static public Task ActAsync(TargetSpiritCtx ctx ) {

			// target spirit chooses to either: 
			return ctx.OtherCtx.SelectActionOption(
				// Add 1 wilds to one of their lands
				new SelfAction("Add 1 wilds to one of your lands", Add1WildsToOneOfYourLands ),
				// Replace 1 of their presence with 1 disease.
				new SelfAction("Replace 1 of your presence with 1 disease", Replace1PresenceWith1Disease)
			);
		}

		static async Task Add1WildsToOneOfYourLands( SelfCtx ctx ) {
			var spaceCtx = await ctx.SelectSpace("Add 1 Wilds",ctx.Self.Presence.Spaces);
			await spaceCtx.Wilds.Add(1);
		}

		static async Task Replace1PresenceWith1Disease( SelfCtx ctx ) {
			var space = await ctx.Presence.SelectDeployed("Replace Presence with 1 disease");
			ctx.Presence.RemoveFrom( space );
			await ctx.Target(space).Disease.Add(1);
		}


	}


}
