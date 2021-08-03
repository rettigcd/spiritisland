using SpiritIsland.Core;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class DahanOnTheirGuard : IFearCard {

		[FearLevel( 1, "In each land, Defend 1 per Dahan." )]
		public Task Level1( GameState gs ) {
			foreach(var space in gs.Island.AllSpaces.Where( gs.HasDahan ))
				gs.Defend( space, gs.GetDahanOnSpace( space ) );
			return Task.CompletedTask;
		}

		// "In each land with Dahan, Defend 1, plus an additional Defend 1 per Dahan.", 
		[FearLevel( 2, "" )]
		public Task Level2( GameState gs ) {
			throw new System.NotImplementedException();
		}

		// "In each land, Defend 2 per Dahan."),
		[FearLevel( 3, "" )]
		Task IFearCard.Level3( GameState gs ){
			throw new System.NotImplementedException();
		}
	}
}
