using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base.Spirits.Shadows {
	class MantleOfDread {

		[SpiritCard("Mantle of Dread",1,Speed.Slow,Element.Moon,Element.Fire,Element.Air)]
		static public async Task Act(ActionEngine engine){

			// target any spirit
			var spirit = await engine.TargetSpirit();

			var (_,gs) = engine;

			// 2 fear
			gs.AddFear(2);

			// target spirit may push 1 explorer and 1 town from land where it has presence
			bool HasExplorerOrTown(Space space){
				var grp = gs.InvadersOn(space);
				return grp.HasExplorer || grp.HasTown;
			}
			// Select Land
			var landsToPushInvadersFrom = spirit.Presence.Where(HasExplorerOrTown).ToArray();
			if(landsToPushInvadersFrom.Length == 0) return;
			var space = await engine.SelectSpace("Select land to push 1 exploer & 1 town from",landsToPushInvadersFrom,true);
			if(space==null) return;

			// duplicated in shadows of the burning forest
			var grp = gs.InvadersOn(space);
			// Push Town
			var townInvaders = grp.Filter("T@2","T@1");
			if(townInvaders.Length>0){
				var townInvader = await engine.SelectInvader("Select town to push",townInvaders,true);
				if(townInvader != null)
					await engine.PushInvader(space,townInvader);
			}
			// Push Explorer
			if(grp.HasExplorer){
				// allow short-circuit
				var explorerInvader = await engine.SelectInvader("Select town to push",grp.Filter("E@1"),true);
				if(explorerInvader != null)
					await engine.PushInvader(space,explorerInvader);
			}
			
		}

	}

}
