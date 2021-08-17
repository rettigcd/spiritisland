using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class TheJungleHungers {

		[MajorCard("The Jungle Hungers",3,Speed.Slow,Element.Moon,Element.Plant)]
		[FromPresenceIn(1,Terrain.Jungle)]
		static public async Task Act(ActionEngine eng){
			// range 1 from presence in jungle
			var target = await eng.Api.TargetSpace( eng, From.Presence, 1, Target.Jungle );
			var grp = eng.InvadersOn(target);

			// destroys all explorers and towns
			grp.Destroy(Invader.Explorer, int.MaxValue);
			grp.Destroy(Invader.Town, int.MaxValue );

			// if you have 2 moon, 3 plant, Destroy 1 city and do not destroy dahan
			if(eng.Self.Elements.Contains( "2 moon,3 plant" )) {
				grp.Destroy( Invader.City, 1 );
			} else {
				int dahanCount = eng.GameState.GetDahanOnSpace(target);
				await eng.GameState.DestoryDahan(target,dahanCount,DahanDestructionSource.PowerCard);
			}

		}

	}
}
