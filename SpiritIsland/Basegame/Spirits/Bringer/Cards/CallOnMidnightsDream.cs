using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class CallOnMidnightsDream {


		[SpiritCard("Call on Midnight's Dream",0, Speed.Fast,Element.Moon,Element.Animal)]
		[FromPresence(0,Target.DahanOrInvaders)] // adding dahan or invaders so that card does something.
		static public async Task ActAsync(TargetSpaceCtx ctx) {

			// if invaders are present, 2 fear
			bool doFear = ctx.HasInvaders
				&& await ctx.Self.UserSelectsFirstText("Select power option","2 fear","Draw Major Power");

			if(doFear)
				ctx.AddFear(2);
			else if(ctx.HasDahan)
				await DrawMajorOrGetEnergy( ctx );
		}

		private static async Task DrawMajorOrGetEnergy( TargetSpaceCtx ctx ) {
			// if target land has dahan, gain a major power.
			var major = await ctx.DrawMajor();
			// If you Forget this Power, gain energy equal to dahan and you may play the major power immediately paying its cost
			if(!ctx.Self.Hand.Contains( major )) // because you discarded it
				ctx.Self.Energy += ctx.DahanCount;
		}
	}
}
