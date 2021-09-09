
using System.Linq;

namespace SpiritIsland.Basegame {
	public class OceanPresence : MyPresence {

		public OceanPresence( PresenceTrack energy, PresenceTrack cardPlays ) : base( energy, cardPlays ) { }

		public override void PlaceOn( Space space ) {
			base.PlaceOn( space );
			// Mark the ocean on this board as a Wetland
			space.Board[0].TerrainForPower = Terrain.Wetland;
		}

		public override void RemoveFrom( Space space ) {
			base.RemoveFrom( space );
			var board = space.Board;
			// If no ocean left on this board
			if(!Spaces.Any(s=>s.Board == board))
				// restore Ocean to an Ocean terrain
				board[0].TerrainForPower = Terrain.Ocean;
		}

	}

}
