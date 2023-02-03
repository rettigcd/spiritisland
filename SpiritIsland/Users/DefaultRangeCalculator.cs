namespace SpiritIsland;

public enum From { None, Presence, SacredSite };

public interface ICalcPowerTargetingSource {
	IEnumerable<SpaceState> FindSources( 
		IKnowSpiritLocations presence, 
		TargetingSourceCriteria criteria, 
		GameState gs					// in case we need extra game data (for Entwined Powers)
	);
}

public interface ICalcRange {

	IEnumerable<SpaceState> GetTargetOptionsFromKnownSource( IEnumerable<SpaceState> source, TargetCriteria targetCriteria );

}

/// <summary>
/// Since Spirit.SourceCalculator is modified by Entwined, use only for Powers
/// </summary>
public class DefaultPowerSourceCalculator : ICalcPowerTargetingSource {
	public IEnumerable<SpaceState> FindSources( IKnowSpiritLocations presence, TargetingSourceCriteria sourceCriteria, GameState gameState ) {
		var sources = sourceCriteria.From switch {
			From.Presence => presence.ActiveSpaceStates,
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
		IEnumerable<SpaceState> sources,
		TargetCriteria targetCriteria
	) {
		return sources
			.SelectMany( x => x.Range( targetCriteria.Range ) ) // !! this will be waistfull when Range is high and lots of starting spots.
			.Distinct()
			.Where( targetCriteria.Matches )
			.ToArray();
	}
	static public readonly ICalcRange Singleton = new DefaultRangeCalculator();
}

