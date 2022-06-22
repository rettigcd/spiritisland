namespace SpiritIsland.Basegame;

// When Ocean is on board,
// changes Ocean-space to a Costal Wetland,
// defers other spaces to original Terrain Mapper
public class OceanTerrainForPower : TerrainMapper {

	public OceanTerrainForPower( TerrainMapper originalMapper, Ocean ocean ) {
		oceanPresence = (OceanPresence)ocean.Presence;
		original = originalMapper;
	}

	// public override Terrain GetTerrain( Space space ) => IsOceansOcean( space ) ? Terrain.Wetland : space.Terrain;
	public override bool MatchesTerrain( Space space, params Terrain[] options ) 
		=> IsOceansOcean( space ) 
			? options.Contains( Terrain.Wetland ) 
			: original.MatchesTerrain( space, options );

	public override bool IsCoastal( Space space ) 
		=> IsOceansOcean( space ) || original.IsCoastal( space );

	#region private

	bool IsOceansOcean( Space space ) => space.IsOcean && oceanPresence.IsOnBoard( space.Board );
	readonly OceanPresence oceanPresence;
	readonly TerrainMapper original;

	#endregion

}