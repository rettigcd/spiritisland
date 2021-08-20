using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class Scapegoats : IFearCard {

		public const string Name = "Scapegoats";

		[FearLevel( 1, "Each Town destroys 1 Explorer in its land." )]
		public Task Level1( GameState gs ) {
			foreach(var space in gs.Island.AllSpaces)
				EachTownDestroys1Explorer( (InvaderGroup)gs.InvadersOn( space ) );
			return Task.CompletedTask;
		}

		[FearLevel( 2, "Each Town destroys 1 Explorer in its land. Each City destroys 2 Explorer in its land." )]
		public Task Level2( GameState gs ) {
			foreach(var space in gs.Island.AllSpaces) {
				var grp = gs.InvadersOn( space );
				EachTownDestroys1AndEachCityDestoys2( (InvaderGroup)grp );
			}
			return Task.CompletedTask;
		}

		[FearLevel( 3, "Destroy all Explorer in lands with Town / City. Each City destroys 1 Town in its land." )]
		public async Task Level3( GameState gs ) {
			foreach(var space in gs.Island.AllSpaces) {
				var grp = (InvaderGroup)gs.InvadersOn( space );
				await grp.Destroy(Invader.Explorer,int.MaxValue);
				await EachCityDestroys1Town( grp );
		
			}
		}

		static Task EachTownDestroys1Explorer( InvaderGroup grp ) {
			return grp.Destroy( Invader.Explorer, grp[Invader.Town] );
		}

		static Task EachCityDestroys1Town( InvaderGroup grp ) {
			return grp.Destroy( Invader.Town, grp[Invader.City] );
		}

		static Task EachTownDestroys1AndEachCityDestoys2( InvaderGroup grp ) {
			int numToDestory = grp[Invader.Town] + grp[Invader.City] * 2;
			return grp.Destroy(Invader.Explorer, numToDestory );
		}


	}
}

