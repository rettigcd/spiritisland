namespace SpiritIsland;

public enum From { None, Presence, SacredSite };

public interface ICalcSource {
	IEnumerable<Space> FindSources( GameState gs, IKnowSpiritLocations presence, TargetSourceCriteria source, TerrainMapper mapper );
}

public interface ICalcRange {

	IEnumerable<Space> GetTargetOptionsFromKnownSource(
		SelfCtx ctx,
		TargettingFrom powerType,
		IEnumerable<SpaceState> source,
		TargetCriteria targetCriteria
	);

}


public class DefaultSourceCalc : ICalcSource {
	public IEnumerable<Space> FindSources( GameState gs, IKnowSpiritLocations presence, TargetSourceCriteria sourceCriteria, TerrainMapper mapper ) {
		var sources = sourceCriteria.From switch {
			From.Presence => presence.Spaces(gs),
			From.SacredSite => presence.SacredSites( gs, mapper ),
			_ => throw new ArgumentException( "Invalid presence source " + sourceCriteria.From ),
		};
		return sources.Where( space => !sourceCriteria.Terrain.HasValue || space.Is( sourceCriteria.Terrain.Value ) );
	}

}


public class DefaultRangeCalculator : ICalcRange {

	public virtual IEnumerable<Space> GetTargetOptionsFromKnownSource(
		SelfCtx ctx,
		TargettingFrom powerType,
		IEnumerable<SpaceState> source,
		TargetCriteria targetCriteria
	) {
		return source
			.SelectMany( x => x.Range( targetCriteria.Range ) )
			.Distinct()
			.Where( s => ctx.Target( s.Space ).Matches( targetCriteria.Filter ) )// matching this destination
			.Select(x=>x.Space); 
	}

	//// Find Range
	//// This is virtual so Volcano Targetting can call base()
	//public virtual IEnumerable<Space> GetTargetOptionsFromKnownSource( 
	//	Spirit self, 
	//	GameState gameState, 
	//	TargettingFrom powerType,
	//	IEnumerable<Space> source,
	//	TargetCriteria targetCriteria
	//) => GetTargetOptionsFromKnownSource(
	//	self.BindMyPower( gameState ), // !!! should this be Power???
	//	powerType,
	//	source,
	//	targetCriteria
	//);

}

