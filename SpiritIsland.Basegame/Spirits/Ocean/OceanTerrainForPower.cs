namespace SpiritIsland.Basegame;

// When Ocean is on board,
// changes Ocean-space to a Costal Wetland,
// defers other spaces to original Terrain Mapper
public class OceanTerrainForPower : TerrainMapper {

	#region constructor

	public OceanTerrainForPower( TerrainMapper originalMapper, Ocean ocean ) {
		_oceanPresence = (OceanPresence)ocean.Presence;
		_original = originalMapper;
	}

	#endregion

	public override bool MatchesTerrain( SpaceState space, params Terrain[] options ) 
		=> _original.MatchesTerrain( space, options ) // try default behavior first
		|| IsOceansOcean( space.Space ) && options.Contains( Terrain.Wetland ); // as backup, check special rule

	public override bool IsCoastal( SpaceState ss ) 
		=> _original.IsCoastal( ss ) // check default 1st
			|| IsOceansOcean( ss.Space ); // if that fails, check special rule

	public override bool IsInPlay( Space space ) 
		=> _original.IsInPlay( space ) 
			|| IsOceansOcean( space );

	#region private

	bool IsOceansOcean( Space space ) {
		return space.IsOcean
			&& GameState.Current.Tokens.IsOn( _oceanPresence.Token, space.Board ); // only call this when actually on ocean
	}
	readonly OceanPresence _oceanPresence;
	readonly TerrainMapper _original;

	#endregion

}