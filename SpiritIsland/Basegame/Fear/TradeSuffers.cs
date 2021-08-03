using SpiritIsland.Core;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class TradeSuffers : IFearCard {

		[FearLevel( 1, "Invaders do not Build in lands with City." )]
		public Task Level1( GameState gs ) {
			gs.SkipBuild( gs.Island.AllSpaces.Where( s => gs.InvadersOn( s ).HasCity ).ToArray() );
			// !! no unit tests on this
			return Task.CompletedTask;
		}

		[FearLevel( 2, "Each player may replace 1 Town with 1 Explorer in a Coastal land." )]
		public Task Level2( GameState gs ) {
			throw new System.NotImplementedException();
		}

		[FearLevel( 3, "Each player may replace 1 City with 1 Town or 1 Town with 1 Explorer in a Coastal land." )]
		public Task Level3( GameState gs ) {
			throw new System.NotImplementedException();
		}

	}

}
