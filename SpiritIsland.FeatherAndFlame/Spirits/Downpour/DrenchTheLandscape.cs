namespace SpiritIsland.FeatherAndFlame;

class DrenchTheLandscape( DownpourDrenchesTheWorld _spirit, TerrainMapper _original ) 
	: TerrainMapper
{

	static public SpecialRule Rule => new SpecialRule(
		"Drench the Landscape",
		"Spirit Actions and Special Rules treat your Sacredsite as Wetlands in addition to the printed terrain."
	);

	public override bool MatchesTerrain( SpaceState space, params Terrain[] options )
		=> _original.MatchesTerrain( space, options )
		|| options.Contains( Terrain.Wetland ) && _spirit.Presence.IsSacredSite( space );

}

