using System.Threading.Tasks;

namespace SpiritIsland.Basegame.Spirits.Bringer {

	public class CallOnMidnightsDream {


		[SpiritCard("Call on Midnight's Dreams",0, Speed.Fast,Element.Moon,Element.Animal)]
		[FromPresence(0,Target.DahanOrInvaders)] // adding dahan or invaders so that card does something.
		static public async Task ActAsync(TargetSpaceCtx ctx) {
			var (self,gs,target) = ctx;

			bool doFear = gs.HasInvaders(target)
				&& await self.SelectFirstText("Select power option","2 fear","Draw Major Power");

			if(doFear)
				// if invaders are present, 2 fear
				ctx.AddFear(2);
			else if(gs.HasDahan( target )) {
				// if target land has dahan, gain a major power.
				var major = await self.CardDrawer.DrawMajor(self,gs,null);
				// If you Forget this Power, gain energy equal to dahan and you may play the major power immediately paying its cost
				if( !self.Hand.Contains(major) ) // because you discarded it
					self.Energy += gs.DahanCount(target);
			}

		}

	}
}
