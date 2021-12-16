using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class FieryVengeance {

		[SpiritCard("Fiery Vengeance",0,Element.Sun,Element.Fire), Fast, AnySpirit]
		static public async Task ActAsync(TargetSpiritCtx ctx ) {

			// Cost to User: Target Spirit Removes 1 of their Destroyed presence from the game.
			if(ctx.Other.Presence.Destroyed==0) return;

			ctx.Other.Presence.RemoveDestroyed(1);

			// 1 fear and 1 damage in one of target Spirit's lands.
			var space = await ctx.Other.Action.Decision(new Select.Space("1 fear + 1 damage", ctx.Other.Presence.Spaces,Present.Always));
			var spaceCtx = ctx.OtherCtx.Target(space); // !!! When we call .OtherCtx, always use the Self Spirits adjustments.
			spaceCtx.AddFear(1);
			await spaceCtx.DamageInvaders(1);
			//  (This is your Power, so blight counts as badland, even if target is another Spirit.)
			
		}

	}

}
