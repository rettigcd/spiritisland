namespace SpiritIsland;

public enum From { None, Presence, SacredSite };

public interface ICalcPowerSource {
	IEnumerable<SpaceState> FindSources( 
		IKnowSpiritLocations presence, 
		TargetSourceCriteria criteria, 
		GameState gs					// in case we need extra game data (for Entwined Powers)
	);
}

public interface ICalcRange {

	IEnumerable<Space> GetTargetOptionsFromKnownSource(
		Spirit self,
		TerrainMapper mapper,
		TargetingPowerType powerType,
		IEnumerable<SpaceState> source,
		TargetCriteria targetCriteria
	);

}

/// <summary>
/// Since Spirit.SourceCalculator is modified by Entwined, use only for Powers
/// </summary>
public class DefaultPowerSourceCalculator : ICalcPowerSource {
	public IEnumerable<SpaceState> FindSources( IKnowSpiritLocations presence, TargetSourceCriteria sourceCriteria, GameState _ ) {
		var sources = sourceCriteria.From switch {
			From.Presence => presence.SpaceStates,
			From.SacredSite => presence.SacredSites,
			_ => throw new ArgumentException( "Invalid presence source " + sourceCriteria.From ),
		};
		return sourceCriteria.Terrain.HasValue
			? sources.Where( space => space.Space.Is( sourceCriteria.Terrain.Value ) )
			: sources;
	}

}

/// <summary>
/// Calculates Ranges using standard Range Rules
/// Pluggable into Spirit to modify how their Power-Range
/// </summary>
public class DefaultRangeCalculator : ICalcRange {

	public virtual IEnumerable<Space> GetTargetOptionsFromKnownSource(
		Spirit self,
		TerrainMapper terrainMapper,
		TargetingPowerType _,

		IEnumerable<SpaceState> sources,
		TargetCriteria targetCriteria

	) {
		//return sources
		//	.SelectMany( x => x.Range( targetCriteria.Range ) )
		//	.Distinct()
		//	.Where( terrainMapper.IsInPlay ) // !important
		//	.Where( SpaceFilterMap.Get( targetCriteria.Filter, self, terrainMapper ) )
		//	.Select(x=>x.Space);

		var a = sources
			.SelectMany( x => x.Range( targetCriteria.Range ) )
			.Distinct()
			.ToArray();
		var b = a.Where( terrainMapper.IsInPlay ).ToArray(); // !important
		var c = b.Where( SpaceFilterMap.Get( targetCriteria.Filter, self, terrainMapper ) ).ToArray();
		return c.Select( x => x.Space );

	}
	static public readonly ICalcRange Singleton = new DefaultRangeCalculator();
}

