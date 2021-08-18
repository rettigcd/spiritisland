using System.Threading.Tasks;

namespace SpiritIsland.Basegame.Spirits.Bringer {

	public class CallOnMidnightsDream {


		[SpiritCard("Call on Midnight's Dreams",0, Speed.Fast,Element.Moon,Element.Animal)]
		[FromPresence(0,Target.DahanOrInvaders)] // adding dahan or invaders so that card does something.
		static public async Task ActAsync(ActionEngine engine, Space target ) {

			bool doFear = engine.GameState.HasInvaders(target)
				&& await engine.SelectFirstText("Select power option","2 fear","Draw Major Power");

			if(doFear)
				// if invaders are present, 2 fear
				engine.AddFear(2);
			else if(engine.GameState.HasDahan( target )) {
				// if target land has dahan, gain a major power.
				var major = await engine.Self.CardDrawer.DrawMajor(engine,null);
				// If you Forget this Power, gain energy equal to dahan and you may play the major power immediately paying its cost
				if( !engine.Self.Hand.Contains(major) ) // because you discarded it
					engine.Self.Energy += engine.GameState.GetDahanOnSpace(target);
			}

		}

	}
}
