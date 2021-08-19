using System.Threading.Tasks;

namespace SpiritIsland.Basegame.Spirits.Bringer {

	public class CallOnMidnightsDream {


		[SpiritCard("Call on Midnight's Dreams",0, Speed.Fast,Element.Moon,Element.Animal)]
		[FromPresence(0,Target.DahanOrInvaders)] // adding dahan or invaders so that card does something.
		static public async Task ActAsync(TargetSpaceCtx ctx) {
			var target = ctx.Target;

			bool doFear = ctx.GameState.HasInvaders(target)
				&& await ctx.Self.SelectFirstText("Select power option","2 fear","Draw Major Power");

			if(doFear)
				// if invaders are present, 2 fear
				ctx.AddFear(2);
			else if(ctx.GameState.HasDahan( target )) {
				// if target land has dahan, gain a major power.
				var major = await ctx.Self.CardDrawer.DrawMajor(ctx.Self,ctx.GameState,null);
				// If you Forget this Power, gain energy equal to dahan and you may play the major power immediately paying its cost
				if( !ctx.Self.Hand.Contains(major) ) // because you discarded it
					ctx.Self.Energy += ctx.GameState.GetDahanOnSpace(target);
			}

		}

	}
}
