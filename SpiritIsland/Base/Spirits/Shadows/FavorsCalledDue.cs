using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base.Spirits.Shadows {

	class FavorsCalledDue {

		[SpiritCard("Favors Called Due",1,Speed.Slow,Element.Moon,Element.Air,Element.Animal)]
		static public async Task Act(ActionEngine engine){
			var (_,gameState) = engine;
			// range 1
			var target = await engine.TargetSpace_Presence(1);

			// gather up to 4 dahan
			await engine.GatherDahan( target, 4 );

			// if invaders are present and dahan now out numberthem, 3 fear
			var invaderCount = engine.GameState.InvadersOn(target).Total;
			if(invaderCount > 0 && gameState.GetDahanOnSpace( target ) > invaderCount) {
				gameState.AddFear( 3 );
			}

		}
	}

}
