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

	public override bool MatchesTerrain( SpaceState space, params Terrain[] options )
		=> original.MatchesTerrain( space, options )
		|| options.Contains( Terrain.Wetland ) && spirit.Presence.IsSacredSite( space );

	IEnumerable<SpaceState> SacredSites(GameState gs) => gs.AllActiveSpaces
		.Where( spirit.Presence.IsSacredSite ); // Downpours SS are not dependent on special terrain rules.
	readonly TerrainMapper defaultTerrainMapper = new TerrainMapper();

	public IEnumerable<SpaceState> FindSources( GameState gs, IKnowSpiritLocations presence, TargetSourceCriteria sourceCriteria, TerrainMapper mapper ) {
		var sources = sourceCriteria.From switch {
			From.Presence => presence.SpaceStates,
			From.SacredSite => SacredSites(gs),
			_ => throw new ArgumentException( "Invalid presence source " + sourceCriteria.From ),
		};
		if(!sourceCriteria.Terrain.HasValue)
			return sources;

		var terrain = sourceCriteria.Terrain.Value;
		var match = sources.Where( space => space.Space.Is( terrain ) );
		return terrain != Terrain.Wetland
			? match
			: match.Union( SacredSites(gs) );

	}
}

