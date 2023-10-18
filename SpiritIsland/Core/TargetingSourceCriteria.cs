namespace SpiritIsland;

public record TargetingSourceCriteria( From From, Terrain? Terrain = null ) {

	/// <summary>
	/// Uses the Target constants to filter source terrain.
	/// </summary>
	/// <remarks>
	/// Doesn't work with Target criteria that rely on current spirit.
	/// </remarks>
	public string Restrict { get; set; }


	public IEnumerable<SpaceState> Filter(IEnumerable<SpaceState> sources ) { 
		if( Terrain.HasValue )
			sources = sources.Where( space => TerrainMapper.Current.MatchesTerrain(space, Terrain.Value) );

		if( Restrict != null)
			sources = sources.Where( new SpaceCriteria( null, Restrict ).Matches );

		return sources;
	}

}
