
using System.Linq;

namespace SpiritIsland.Basegame {

	public class OceanPresence : MyPresence {

		public OceanPresence( PresenceTrack energy, PresenceTrack cardPlays ) : base( energy, cardPlays ) { }

		public override void PlaceOn( Space space ) {
			base.PlaceOn( space );
			MakeOceanCostalWetland( space.Board[0] );
		}

		public override void RemoveFrom( Space space ) {
			base.RemoveFrom( space );
			var board = space.Board;
			if(!Spaces.Any(s=>s.Board == board ))
				RestoreOcean( board[0] );
		}

		static void RestoreOcean( Space ocean ) {
			ocean.TerrainForPower = Terrain.Ocean;
			ocean.IsCostalForPower = true;
		}

		static void MakeOceanCostalWetland( Space ocean ) {
			ocean.TerrainForPower = Terrain.Wetland;
			ocean.IsCostalForPower = true;
		}

	}

}
