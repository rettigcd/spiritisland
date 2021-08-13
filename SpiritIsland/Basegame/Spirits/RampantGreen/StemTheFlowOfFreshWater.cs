using SpiritIsland.Core;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class StemTheFlowOfFreshWater {
		[SpiritCard( "STep the Flow of Fresh Water", 0, Speed.Slow, Element.Water, Element.Plant )]
		[FromSacredSite( 1 )]
		static public async Task ActionAsync( ActionEngine eng, Space target ) {

			var grp = eng.GameState.InvadersOn(target);

			bool damageMany = target.Terrain.IsIn( Terrain.Mountain, Terrain.Sand );

			void damageAll(Invader invader) { while(grp[invader]>0) grp.ApplyDamageTo1(invader,1); }


			if(damageMany) {
				// If target land is mountain or sand, instead 1 damange to EACH town/city

				// order from damaged to healthy to prevent cascading damage
				damageAll( Invader.City1);
				damageAll( Invader.City2 );
				damageAll( Invader.City );
				damageAll( Invader.Town1 );
				damageAll( Invader.Town );
			} else {
				// 1 damage to 1 town or city.
				var types = grp.FilterBy(Invader.City,Invader.Town);
				var invader = await eng.SelectInvader("1 damage to",types);
				if(invader !=null)
					grp.ApplyDamageTo1(invader,1);
			}

		}

	}

}
