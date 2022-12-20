namespace SpiritIsland;

public enum From { None, Presence, SacredSite };

public interface ICalcPowerTargetingSource {
	IEnumerable<SpaceState> FindSources( 
		IKnowSpiritLocations presence, 
		TargetSourceCriteria criteria, 
		GameState gs					// in case we need extra game data (for Entwined Powers)
	);
}

public interface ICalcRange {

	// !!! Also, return SpaceState instead of Space.
	IEnumerable<SpaceState> GetTargetOptionsFromKnownSource(
		Spirit self,
		TerrainMapper mapper,			// !!! This is only needed when we are passing in TargetCriteria that uses it.  We could eliminate it by merging it into the TargetCriteria (when needed) and ignore it when not.
		TargetingPowerType powerType,
		IEnumerable<SpaceState> source,
		TargetCriteria targetCriteria	// !!! when we have a filter criteria, this could bind to and encapsulate the terrain-mapper to hide it. (And not have to pass it around to everything)
	);

}

/// <summary>
/// Since Spirit.SourceCalculator is modified by Entwined, use only for Powers
/// </summary>
public class DefaultPowerSourceCalculator : ICalcPowerTargetingSource {
	public IEnumerable<SpaceState> FindSources( IKnowSpiritLocations presence, TargetSourceCriteria sourceCriteria, GameState gameState ) {
		var sources = sourceCriteria.From switch {
			From.Presence => presence.SpaceStates,
			From.SacredSite => presence.SacredSites,
			_ => throw new ArgumentException( "Invalid presence source " + sourceCriteria.From ),
		};
		return sourceCriteria.Terrain.HasValue
			? sources.Where( space => gameState.Island.Terrain_ForPower.MatchesTerrain( space, sourceCriteria.Terrain.Value ) )
			: sources;
	}

}

/// <summary>
/// Calculates Ranges using standard Range Rules
/// Pluggable into Spirit to modify how their Power-Range
/// </summary>
public class DefaultRangeCalculator : ICalcRange {

	public virtual IEnumerable<SpaceState> GetTargetOptionsFromKnownSource(
		Spirit self,
		TerrainMapper terrainMapper,
		TargetingPowerType _,

		IEnumerable<SpaceState> sources,
		TargetCriteria targetCriteria

	) {
		return sources
			.SelectMany( x => x.Range( targetCriteria.Range ) ) // !! this will be waistfull when Range is high and lots of starting spots.
			.Distinct()
			.Where( terrainMapper.IsInPlay )
			.Where( targetCriteria.Bind( self, terrainMapper ) )
			.ToArray();
	}
	static public readonly ICalcRange Singleton = new DefaultRangeCalculator();
}

