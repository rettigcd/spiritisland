namespace SpiritIsland.FeatherAndFlame;

class DrenchTheLandscape : TerrainMapper
{

	static public SpecialRule Rule => new SpecialRule(
		"Drench the Landscape",
		"Spirit Actions and Special Rules treat your Sacredsite as Wetlands in addition to the printed terrain."
	);

	readonly DownpourDrenchesTheWorld _spirit;
	readonly TerrainMapper _original;

	public DrenchTheLandscape( DownpourDrenchesTheWorld spirit, TerrainMapper original ) {
		_spirit = spirit;
		_original = original;
	}

	public override bool MatchesTerrain( SpaceState space, params Terrain[] options )
		=> _original.MatchesTerrain( space, options )
		|| options.Contains( Terrain.Wetland ) && _spirit.Presence.IsSacredSite( space );

}

