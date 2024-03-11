namespace SpiritIsland;

public enum TargetFrom { 
	None, 
	Presence, 
	SacredSite, 
	SuperSacredSite,
	Incarna,
};

public interface ITargetingSourceStrategy {
	IEnumerable<SpaceState> EvaluateFrom( IKnowSpiritLocations presence, TargetFrom from );
}

public interface ICalcRange {
	IEnumerable<SpaceState> GetSpaceOptions( IEnumerable<SpaceState> source, params TargetCriteria[] targetCriteria );
	IEnumerable<SpaceState> GetSpaceOptions( SpaceState source, TargetCriteria tc );
}

/// <summary>
/// Since Spirit.SourceCalculator is modified by Entwined, use only for Powers
/// </summary>
public class DefaultPowerSourceStrategy : ITargetingSourceStrategy {
	// ! Should work for any action because we are now referencing TerrainMapper.Current instead of directly accessing the ForPower one.

	public IEnumerable<SpaceState> EvaluateFrom( IKnowSpiritLocations presence, TargetFrom from ) {
		return from switch {
			TargetFrom.Presence => presence.Lands,
			TargetFrom.SacredSite => presence.SacredSites,
			TargetFrom.SuperSacredSite => presence.SuperSacredSites,
			TargetFrom.Incarna => presence is SpiritPresence sp && sp.Incarna.IsPlaced // !! Maybe IKnowSpiritLocations should have an IEnumerable<SpaceState> InvarnaLocations;
				? new SpaceState[] { sp.Incarna.Space } 
				: [],
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

