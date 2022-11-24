namespace SpiritIsland;

public enum From { None, Presence, SacredSite };

public interface ICalcSource {
	IEnumerable<SpaceState> FindSources( GameState gs, IKnowSpiritLocations presence, TargetSourceCriteria source, TerrainMapper mapper );
}

public interface ICalcRange {

	IEnumerable<Space> GetTargetOptionsFromKnownSource(
		SelfCtx ctx,
		TargetingPowerType powerType,
		IEnumerable<SpaceState> source,
		TargetCriteria targetCriteria
	);

}


public class DefaultSourceCalc : ICalcSource {
	public IEnumerable<SpaceState> FindSources( GameState gs, IKnowSpiritLocations presence, TargetSourceCriteria sourceCriteria, TerrainMapper _ ) {
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


public class DefaultRangeCalculator : ICalcRange {

	public virtual IEnumerable<Space> GetTargetOptionsFromKnownSource(

		SelfCtx ctx,
		TargetingPowerType _,

		IEnumerable<SpaceState> sources,
		TargetCriteria targetCriteria

	) {
		return sources
			.SelectMany( x => x.Range( targetCriteria.Range ) )
			.Distinct()
			.Where( SpaceFilterMap.Get( targetCriteria.Filter, ctx.Self, ctx.TerrainMapper ) )
			.Select(x=>x.Space); 
	}

}

