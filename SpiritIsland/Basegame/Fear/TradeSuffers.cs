using SpiritIsland.Core;
using System.Linq;

namespace SpiritIsland.Basegame {

	public class TradeSuffers : IFearCard {

		//"Invaders do not Build in lands with City.", 
		public void Level1( GameState gs ) {
			gs.SkipBuild( gs.Island.AllSpaces.Where( s => gs.InvadersOn( s ).HasCity ).ToArray() );
			// !! no unit tests on this
		}

		//"Each player may replace 1 Town with 1 Explorer in a Coastal land.", 
		public void Level2( GameState gs ) {
			throw new System.NotImplementedException();
		}

		//"Each player may replace 1 City with 1 Town or 1 Town with 1 Explorer in a Coastal land."),
		public void Level3( GameState gs ) {
			throw new System.NotImplementedException();
		}

	}

}
