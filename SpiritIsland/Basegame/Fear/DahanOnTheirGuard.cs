using SpiritIsland.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class DahanOnTheirGuard : IFearCard {

		[FearLevel( 1, "In each land, Defend 1 per Dahan." )]
		public Task Level1( GameState gs ) {
			int defend( Space space ) => gs.GetDahanOnSpace( space );
			return DefendIt( gs, defend );
		}

		// "In each land with Dahan, Defend 1, plus an additional Defend 1 per Dahan.", 
		[FearLevel( 2, "" )]
		public Task Level2( GameState gs ) {
			int defend(Space space) => 1 + gs.GetDahanOnSpace( space );
			return DefendIt( gs, defend );
		}

		// "In each land, Defend 2 per Dahan."),
		[FearLevel( 3, "" )]
		Task IFearCard.Level3( GameState gs ){
			int defend( Space space ) => 2* gs.GetDahanOnSpace( space );
			return DefendIt( gs, defend );
		}

		static Task DefendIt( GameState gs, Func<Space, int> d ) {
			foreach(var space in gs.Island.AllSpaces.Where( gs.HasDahan ))
				gs.Defend( space, d( space ) );
			return Task.CompletedTask;
		}
	}

}
