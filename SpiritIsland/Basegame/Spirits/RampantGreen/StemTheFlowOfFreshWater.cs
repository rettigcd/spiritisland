using SpiritIsland;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class StemTheFlowOfFreshWater {
		[SpiritCard( "Stem the Flow of Fresh Water", 0, Speed.Slow, Element.Water, Element.Plant )]
		[FromSacredSite( 1 )]
		static public async Task ActionAsync( ActionEngine eng, Space target ) {

			var grp = eng.GameState.InvadersOn(target);

			// If target land is mountain or sand, 
			if( target.Terrain.IsIn( Terrain.Mountain, Terrain.Sand ) ) {
				// instead 1 damange to EACH town/city
				grp.ApplyDamageToEach(1,Invader.City,Invader.Town);
			} else {
				// 1 damage to 1 town or city.
				var types = grp.FilterBy(Invader.City,Invader.Town);
				var invader = await eng.SelectInvader("1 damage to",types);
				if(invader !=null)
					grp.ApplyDamageTo1(1,invader);
			}

		}

	}

}
