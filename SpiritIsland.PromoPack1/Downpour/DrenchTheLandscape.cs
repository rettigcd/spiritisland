namespace SpiritIsland.PromoPack1;

class DrenchTheLandscape : TerrainMapper, ICalcSource {

	static public SpecialRule Rule => new SpecialRule(
		"Drench the Landscape",
		"Spirit Actions and Special Rules treat your Sacredsite as Wetlands in addition to the printed terrain."
	);

	readonly DownpourDrenchesTheWorld spirit;
	readonly TerrainMapper original;

	public DrenchTheLandscape( DownpourDrenchesTheWorld spirit, TerrainMapper original ) {
		this.spirit = spirit;
		this.original = original;
	}

	public override bool MatchesTerrain( Space space, params Terrain[] options )
		=> original.MatchesTerrain( space, options )
		|| options.Contains( Terrain.Wetland ) && spirit.Presence.SacredSites.Contains( space );

	public IEnumerable<Space> FindSources( IKnowSpiritLocations presence, TargetSourceCriteria sourceCriteria ) {
		// !! If we had access to the GameState or Island here, we could just pull use the TerrainMapper_ForPower that is attached to the island.

		var sources = sourceCriteria.From switch {
			From.Presence => presence.Spaces,
			From.SacredSite => presence.SacredSites,
			_ => throw new ArgumentException( "Invalid presence source " + sourceCriteria.From ),
		};
		if(!sourceCriteria.Terrain.HasValue)
			return sources;

		var terrain = sourceCriteria.Terrain.Value;
		var match = sources.Where( space => space.Is( terrain ) );
		return terrain != Terrain.Wetland
			? match
			: match.Union( presence.SacredSites );

	}

}

