using System.Linq;

namespace SpiritIsland.Basegame {
	public class OceanTerrainForPower : TerrainMapper {

		readonly OceanPresence oceanPresence;

		public OceanTerrainForPower( Ocean ocean ) {
			oceanPresence = (OceanPresence)ocean.Presence;
		}

		// public override Terrain GetTerrain( Space space ) => IsOceansOcean( space ) ? Terrain.Wetland : space.Terrain;
		public override bool IsOneOf( Space space, params Terrain[] options ) 
			=> IsOceansOcean( space ) 
				? options.Contains( Terrain.Wetland ) 
				: base.IsOneOf( space, options );

		public override bool IsCoastal( Space space ) 
			=> IsOceansOcean( space ) || space.IsCoastal;

		bool IsOceansOcean( Space space ) {
			return space.IsOcean
				&& oceanPresence.IsOnBoard( space.Board );
		}

	}


}
