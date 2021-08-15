using SpiritIsland;
using System;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class Scapegoats : IFearCard {

		[FearLevel( 1, "Each Town destroys 1 Explorer in its land." )]
		public Task Level1( GameState gs ) {
			foreach(var space in gs.Island.AllSpaces)
				EachTownDestroys1Explorer( gs.InvadersOn( space ) );
			return Task.CompletedTask;
		}

		[FearLevel( 2, "Each Town destroys 1 Explorer in its land. Each City destroys 2 Explorer in its land." )]
		public Task Level2( GameState gs ) {
			foreach(var space in gs.Island.AllSpaces) {
				var grp = gs.InvadersOn( space );
				EachTownDestroys1AndEachCityDestoys2( grp );
			}
			return Task.CompletedTask;
		}

		[FearLevel( 3, "Destroy all Explorer in lands with Town / City. Each City destroys 1 Town in its land." )]
		public Task Level3( GameState gs ) {
			foreach(var space in gs.Island.AllSpaces) {
				var grp = gs.InvadersOn( space );
				grp.Destroy(Invader.Explorer,int.MaxValue);
				EachCityDestroys1Town( grp );
		
			}
			return Task.CompletedTask;
		}

		static void EachTownDestroys1Explorer( InvaderGroup grp ) {
			grp.Destroy( Invader.Explorer, grp[Invader.Town] );
		}

		static void EachCityDestroys1Town( InvaderGroup grp ) {
			grp.Destroy( Invader.Town, grp[Invader.City] );
		}

		static void EachTownDestroys1AndEachCityDestoys2( InvaderGroup grp ) {
			int numToDestory = grp[Invader.Town] + grp[Invader.City] * 2;
			grp.Destroy(Invader.Explorer, numToDestory );
		}


	}
}

