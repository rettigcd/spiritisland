namespace SpiritIsland;

/// <summary>
/// Since Spirit.SourceCalculator is modified by Entwined, use only for Powers
/// </summary>
public class DefaultPowerSourceStrategy : ITargetingSourceStrategy {
	// ! Should work for any action because we are now referencing TerrainMapper.Current instead of directly accessing the ForPower one.

	public IEnumerable<Space> EvaluateFrom( IKnowSpiritLocations presence, TargetFrom from ) {
		return from switch {
			TargetFrom.Presence => presence.Lands,
			TargetFrom.SacredSite => presence.SacredSites,
			TargetFrom.Incarna => presence is SpiritPresence sp && sp.Incarna.IsPlaced // !! Maybe IKnowSpiritLocations should have an IEnumerable<Space> InvarnaLocations;
				? new Space[] { sp.Incarna.Space } 
				: [],
			_ => throw new ArgumentException( "Invalid presence source " + from ),
		};
	}
}

