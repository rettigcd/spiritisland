using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class Scapegoats : IFearCard {

		public const string Name = "Scapegoats";

		[FearLevel( 1, "Each Town destroys 1 Explorer in its land." )]
		public Task Level1( GameState gs ) {
			foreach(var space in gs.Island.AllSpaces)
				Destroy_1ExplorerPerTown( gs.Invaders.On( space, Cause.Fear ) );
			return Task.CompletedTask;
		}

		[FearLevel( 2, "Each Town destroys 1 Explorer in its land. Each City destroys 2 Explorer in its land." )]
		public Task Level2( GameState gs ) {
			foreach(var space in gs.Island.AllSpaces) {
				var grp = gs.Invaders.On( space, Cause.Fear );
				Destory_1ExplorerPerTownAnd2ExplorersPerCity( grp );
			}
			return Task.CompletedTask;
		}

		[FearLevel( 3, "Destroy all Explorer in lands with Town / City. Each City destroys 1 Town in its land." )]
		public async Task Level3( GameState gs ) {
			foreach(var space in gs.Island.AllSpaces) {
				var grp = (InvaderGroup)gs.Invaders.On( space, Cause.Fear );
				await grp.Destroy( Invader.Explorer, int.MaxValue );
				await EachCityDestroys1Town( grp );
		
			}
		}

		static Task Destroy_1ExplorerPerTown( InvaderGroup grp ) {
			return grp.Destroy( Invader.Explorer, grp.Counts.Sum(Invader.Town) );
		}

		static Task EachCityDestroys1Town( InvaderGroup grp ) {
			return grp.Destroy( Invader.Town, grp.Counts.Sum(Invader.City) );
		}

		static Task Destory_1ExplorerPerTownAnd2ExplorersPerCity( InvaderGroup grp ) {
			int numToDestory = grp.Counts.Sum(Invader.Town) + grp.Counts.Sum(Invader.City) * 2;
			return grp.Destroy( Invader.Explorer, numToDestory );
		}


	}
}

