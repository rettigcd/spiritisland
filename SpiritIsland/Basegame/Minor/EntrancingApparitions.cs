using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	class EntrancingApparitions {

		[MinorCard("Entrancing Apparitions",1,Speed.Fast,Element.Moon,Element.Air,Element.Water)]
		[FromPresence(1)]
		static public async Task Act(ActionEngine engine,Space target){
			var (spirit,gs) = engine;
			// defend 2
			gs.Defend(target,2);

			// if no invaders are present, gather 2 explorers
			if(gs.HasInvaders(target)) return;

			int remaining = 2;
			Space[] CalcSpaceOptions() => target.Adjacent.Where(n=>gs.InvadersOn(n).HasExplorer).ToArray();
			Space[] spacesWithExplorers = CalcSpaceOptions();
			while(remaining>0 && spacesWithExplorers.Length>0){
				var source = await engine.SelectSpace("pull explorer from",spacesWithExplorers,true);
				if(source==null) break;
				gs.Move(InvaderSpecific.Explorer,source,target);
				--remaining;
				spacesWithExplorers = CalcSpaceOptions();;
			}

		}

	}
}
