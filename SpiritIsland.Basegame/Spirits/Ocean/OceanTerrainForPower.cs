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
		=> original.MatchesTerrain( space, options ) // try default behavior first
		|| IsOceansOcean( space ) && options.Contains( Terrain.Wetland ); // as backup, check special rule

	public override bool IsCoastal( SpaceState ss ) 
		=> original.IsCoastal( ss ) // check default 1st
			|| IsOceansOcean( ss ); // if that fails, check special rule

	public override bool IsInPlay( SpaceState spaceState ) 
		=> original.IsInPlay( spaceState ) 
			|| IsOceansOcean( spaceState );

	#region private

	bool IsOceansOcean( SpaceState spaceState ) {
		return spaceState.Space.IsOcean
			&& spaceState.HasTokenOnBoard( oceanPresence.Token ); // only call this when actually on ocean
	}
	readonly OceanPresence oceanPresence;
	readonly TerrainMapper original;

	#endregion

}