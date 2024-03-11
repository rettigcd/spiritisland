namespace SpiritIsland.Basegame;

// When Ocean is on board,
// changes Ocean-space to a Costal Wetland,
// defers other spaces to original Terrain Mapper
public class OceanTerrainForPower( TerrainMapper _originalMapper, Ocean _ocean ) : TerrainMapper {

	#region constructor

	#endregion

	public override bool MatchesTerrain( Space space, params Terrain[] options ) 
		=> _originalMapper.MatchesTerrain( space, options ) // try default behavior first
		|| IsOceansOcean( space ) && options.Contains( Terrain.Wetland ); // as backup, check special rule

	public override bool IsCoastal( Space ss ) 
		=> _originalMapper.IsCoastal( ss ) // check default 1st
			|| IsOceansOcean( ss ); // if that fails, check special rule

	public override bool IsInPlay(Space space)
		=> _originalMapper.IsInPlay(space) || IsOceansOcean( space );

	#region private

	bool IsOceansOcean( Space space ) {
		return space.SpaceSpec.IsOcean
			&& space.SpaceSpec.Boards.Any( _oceanPresence.Token.IsOn ); // only call this when actually on ocean
	}
	readonly OceanPresence _oceanPresence = (OceanPresence)_ocean.Presence;

	#endregion

}