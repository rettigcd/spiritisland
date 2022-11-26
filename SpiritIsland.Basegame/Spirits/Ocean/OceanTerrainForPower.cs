namespace SpiritIsland.Basegame;

// When Ocean is on board,
// changes Ocean-space to a Costal Wetland,
// defers other spaces to original Terrain Mapper
public class OceanTerrainForPower : TerrainMapper {

	#region constructor

	public OceanTerrainForPower( TerrainMapper originalMapper, Ocean ocean ) {
		oceanPresence = (OceanPresence)ocean.Presence;
		original = originalMapper;
	}

	#endregion

	public override bool MatchesTerrain( SpaceState space, params Terrain[] options ) 
		=> IsOceansOcean( space.Space ) 
			? options.Contains( Terrain.Wetland ) 
			: original.MatchesTerrain( space, options );

	public override bool IsCoastal( Space space ) 
		=> IsOceansOcean( space ) || original.IsCoastal( space );

	public override bool IsInPlay( Space space ) => IsOceansOcean( space ) || original.IsInPlay( space );

	#region private

	bool IsOceansOcean( Space space ) {
		bool a = space.IsOcean;
		bool b = oceanPresence.IsOnBoard( space.Board );
		return a && b;
		// return space.IsOcean && oceanPresence.IsOnBoard( space.Board );
	}
	readonly OceanPresence oceanPresence;
	readonly TerrainMapper original;

	#endregion

}