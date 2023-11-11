namespace SpiritIsland;

public enum From { 
	None, 
	Presence, 
	SacredSite, 
	SuperSacredSite,
	Incarna,
};

public interface ITargetingSourceStrategy {
	IEnumerable<SpaceState> EvaluateFrom( IKnowSpiritLocations presence, From from );
}

public interface ICalcRange {
	IEnumerable<SpaceState> GetSpaceOptions( IEnumerable<SpaceState> source, params TargetCriteria[] targetCriteria );
	IEnumerable<SpaceState> GetSpaceOptions( SpaceState source, TargetCriteria tc );
}

public interface IIncarnaToken : IToken, IAppearInSpaceAbreviation {
	SpaceState Space { get; }
	bool Empowered { get; set; }
}


public interface IHaveIncarna {
	public IIncarnaToken Incarna { get; }
}


/// <summary>
/// Since Spirit.SourceCalculator is modified by Entwined, use only for Powers
/// </summary>
public class DefaultPowerSourceStrategy : ITargetingSourceStrategy {
	// ! Should work for any action because we are now referencing TerrainMapper.Current instead of directly accessing the ForPower one.

	public IEnumerable<SpaceState> EvaluateFrom( IKnowSpiritLocations presence, From from ) {
		return from switch {
			From.Presence => presence.Spaces.Tokens(),
			From.SacredSite => presence.SacredSites,
			From.SuperSacredSite => presence.SuperSacredSites,
			From.Incarna => presence is IHaveIncarna incarnaHolder && incarnaHolder.Incarna.Space is not null 
				? new SpaceState[] { incarnaHolder.Incarna.Space } 
				: Array.Empty<SpaceState>(),
			_ => throw new ArgumentException( "Invalid presence source " + from ),
		};
	}
}

/// <summary>
/// Calculates Ranges using standard Range Rules
/// Pluggable into Spirit to modify how their Power-Range
/// </summary>
public class DefaultRangeCalculator : ICalcRange {

	public IEnumerable<SpaceState> GetSpaceOptions(
		IEnumerable<SpaceState> sources,
		params TargetCriteria[] targetCriteria
	) {
		return sources
			.SelectMany( source => GetSpaceOptionsForCriteria( source, targetCriteria ) )
			.Distinct()
			.ToArray();
	}

	IEnumerable<SpaceState> GetSpaceOptionsForCriteria(SpaceState source, params TargetCriteria[] targetCriteria)
		=> targetCriteria.Length == 1
			? GetSpaceOptions( source, targetCriteria[0] ) 
			: targetCriteria.SelectMany( tc => GetSpaceOptions( source, tc ) ).Distinct();

	public virtual IEnumerable<SpaceState> GetSpaceOptions(SpaceState source, TargetCriteria tc)
		=> source.Range(tc.Range).Where( tc.Matches );

	static public readonly ICalcRange Singleton = new DefaultRangeCalculator();
}

