using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	class TheJungleHungers {

		[MajorCard("The Jungle Hungers",3,Speed.Slow,Element.Moon,Element.Plant)]
		static public async Task Act(ActionEngine eng){
			// range 1 from presence in jungle
			var target = await eng.Api.TargetSpace( eng, eng.Self.Presence.Spaces.Where(s=>s.Terrain==Terrain.Jungle),1);
			InvaderGroup grp = eng.GameState.InvadersOn(target);

			bool hasBonus = 2<=eng.Self.Elements[Element.Moon] 
				&& 3<=eng.Self.Elements[Element.Plant];

			// destroys all explorers and towns
			grp.DestroyType(Invader.Explorer, int.MaxValue);
			grp.DestroyType(Invader.Town, int.MaxValue );

			// if you have 2 moon, 3 plant, Destroy 1 city and do not destroy dahan
			if(!hasBonus){
				int dahanCount = eng.GameState.GetDahanOnSpace(target);
				await eng.GameState.DestoryDahan(target,dahanCount,DahanDestructionSource.PowerCard);
			} else {
				grp.DestroyType(Invader.City, 1);
			}

		}

	}
}
