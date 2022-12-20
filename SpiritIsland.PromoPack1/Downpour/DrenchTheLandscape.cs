namespace SpiritIsland.PromoPack1;

class DrenchTheLandscape : TerrainMapper, ICalcPowerTargetingSource {

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

	public IEnumerable<SpaceState> FindSources( 
		IKnowSpiritLocations presence, 
		TargetSourceCriteria sourceCriteria,
		GameState gs
	) {
		var sources = sourceCriteria.From switch {
			From.Presence => presence.SpaceStates,
			From.SacredSite => SacredSites(gs),
			_ => throw new ArgumentException( "Invalid presence source " + sourceCriteria.From ),
		};

		// !!! This only treats Spaces as Wetlands for Targeting-Source and not for everything else that tests for Wetlands.
		// !!! If we set all the TerrainMappers correctly, then we wouldn't need to override CalcSource

		return sourceCriteria.Terrain.HasValue
			// If we are filltering on water
			? sourceCriteria.Terrain.Value == Terrain.Wetland
				// Add Sacred Sites in, explicitly
				? sources.Where( space => space.Space.Is( sourceCriteria.Terrain.Value ) ).Union( SacredSites( gs ) )
				: sources.Where( space => space.Space.Is( sourceCriteria.Terrain.Value ) )
			: sources;
	}

}

