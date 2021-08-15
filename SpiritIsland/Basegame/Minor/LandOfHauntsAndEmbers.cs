using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	class LandOfHauntsAndEmbers {

		[MinorCard("Land of Haunts and Embers",0,Speed.Fast,Element.Moon,Element.Fire,Element.Air)]
		[FromPresence(2)]
		static public async Task Act(ActionEngine engine,Space target){
			
			// 2 fear
			engine.GameState.AddFear(2);

			bool hasBlight = engine.GameState.HasBlight(target);
			// if target has blight
			if(hasBlight){
				// +2 fear
				engine.GameState.AddFear(2);
				// add 1 blight
				engine.GameState.AddBlight(target,1);
			}

			// push up to 2 more explorers/towns, add 1 blight
			int pushCount = hasBlight ? 4 : 2;
			await engine.PushUpToNInvaders(target,pushCount,Invader.Explorer,Invader.Town);
		}

	}
}
