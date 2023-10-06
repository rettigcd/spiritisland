namespace SpiritIsland;

public enum From { 
	None, 
	Presence, 
	SacredSite, 
	Incarna,
};

public interface ICalcPowerTargetingSource {
	IEnumerable<SpaceState> FindSources( 
		IKnowSpiritLocations presence, 
		TargetingSourceCriteria criteria
	);
}

public interface ICalcRange {

	IEnumerable<SpaceState> GetTargetOptionsFromKnownSource( IEnumerable<SpaceState> source, TargetCriteria targetCriteria );

}

public interface IIncarnaToken : IToken, IAppearInSpaceAbreviation {
	SpaceState? Space { get; }
	bool Empowered { get; set; }
}


public interface IHaveIncarna {
	public IIncarnaToken Incarna { get; }
}


/// <summary>
/// Since Spirit.SourceCalculator is modified by Entwined, use only for Powers
/// </summary>
public class DefaultPowerSourceCalculator : ICalcPowerTargetingSource {
	// ! Should work for any action because we are now referencing TerrainMapper.Current instead of directly accessing the ForPower one.

	public IEnumerable<SpaceState> FindSources( IKnowSpiritLocations presence, TargetingSourceCriteria sourceCriteria ) {
		var sources = sourceCriteria.From switch {
			From.Presence => presence.Spaces.Tokens(),
			From.SacredSite => presence.SacredSites,
			From.Incarna => presence is IHaveIncarna incarnaHolder && incarnaHolder.Incarna.Space is not null ? new SpaceState[]{ incarnaHolder.Incarna.Space } : new SpaceState[0],
			_ => throw new ArgumentException( "Invalid presence source " + sourceCriteria.From ),
		};
		return sourceCriteria.Terrain.HasValue
			? sources.Where( space => TerrainMapper.Current.MatchesTerrain( space, sourceCriteria.Terrain.Value ) )
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

