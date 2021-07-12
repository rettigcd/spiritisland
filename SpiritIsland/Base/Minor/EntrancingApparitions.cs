using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	class EntrancingApparitions {

		[MinorCard("Entrancing Apparitions",1,Speed.Fast,Element.Moon,Element.Air,Element.Water)]
		static public async Task Act(ActionEngine engine){
			var (spirit,gs) = engine;
			var target = await engine.TargetSpace_Presence(1);
			// defend 2
			gs.Defend(target,2);

			// if no invaders are present, gather 2 explorers
			if(gs.HasInvaders(target)) return;

			int remaining = 2;
			Space[] CalcSpaceOptions() => target.Neighbors.Where(n=>gs.InvadersOn(n).HasExplorer).ToArray();
			Space[] spacesWithExplorers = CalcSpaceOptions();
			while(remaining>0 && spacesWithExplorers.Length>0){
				var source = await engine.SelectSpace("pull explorer from",spacesWithExplorers,true);
				if(source==null) break;
				gs.Adjust(target,Invader.Explorer,1);
				gs.Adjust(source,Invader.Explorer,-1);
				--remaining;
				spacesWithExplorers = CalcSpaceOptions();;
			}

		}

	}
}
