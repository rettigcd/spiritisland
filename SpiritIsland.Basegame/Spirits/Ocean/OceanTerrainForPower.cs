namespace SpiritIsland.Basegame {
	public class OceanTerrainForPower : TerrainMapper {

		OceanPresence oceanPresence;

		public OceanTerrainForPower( Ocean ocean ) {
			oceanPresence = (OceanPresence)ocean.Presence;
		}

		public override Terrain GetTerrain( Space space )
			=> IsOceansOcean( space ) ? Terrain.Wetland : space.Terrain;


		public override bool IsCoastal( Space space ) 
			=> IsOceansOcean( space ) || space.IsCoastal;

		bool IsOceansOcean( Space space ) {
			return space.Terrain == Terrain.Ocean
				&& oceanPresence.IsOnBoard( space.Board );
		}

	}


}
