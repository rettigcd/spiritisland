namespace SpiritIsland;

public enum From { 
	None, 
	Presence, 
	SacredSite, 
	Incarna,
};

public interface ITargetingSourceStrategy {
	IEnumerable<SpaceState> EvaluateFrom( IKnowSpiritLocations presence, From from );
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
public class DefaultPowerSourceStrategy : ITargetingSourceStrategy {
	// ! Should work for any action because we are now referencing TerrainMapper.Current instead of directly accessing the ForPower one.

	public IEnumerable<SpaceState> EvaluateFrom( IKnowSpiritLocations presence, From from ) {
		return from switch {
			From.Presence => presence.Spaces.Tokens(),
			From.SacredSite => presence.SacredSites,
			From.Incarna => presence is IHaveIncarna incarnaHolder && incarnaHolder.Incarna.Space is not null ? new SpaceState[] { incarnaHolder.Incarna.Space } : new SpaceState[0],
			_ => throw new ArgumentException( "Invalid presence source " + from ),
		};
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

