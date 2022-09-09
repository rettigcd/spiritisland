namespace SpiritIsland;

#region Source
public enum From { None, Presence, SacredSite };

public interface ICalcSource {
	IEnumerable<Space> FindSources( IKnowSpiritLocations presence, TargetSourceCriteria source, TerrainMapper mapper );

}

public class DefaultSourceCalc : ICalcSource {
	public virtual IEnumerable<Space> FindSources( IKnowSpiritLocations presence, TargetSourceCriteria sourceCriteria, TerrainMapper mapper ) {
		var sources = sourceCriteria.From switch {
			From.Presence => presence.Spaces,
			From.SacredSite => presence.SacredSites(mapper),
			_ => throw new ArgumentException( "Invalid presence source " + sourceCriteria.From ),
		};
		return sources.Where( space => !sourceCriteria.Terrain.HasValue || space.Is( sourceCriteria.Terrain.Value ) );
	}
}

#endregion

#region Range

public interface ICalcRange {

	IEnumerable<Space> GetTargetOptionsFromKnownSource( 
		SelfCtx ctx,
		TargettingFrom powerType,
		IEnumerable<Space> source,
		TargetCriteria targetCriteria
	);

}

public class DefaultRangeCalculator : ICalcRange {

	public virtual IEnumerable<Space> GetTargetOptionsFromKnownSource(
		SelfCtx ctx,
		TargettingFrom powerType,
		IEnumerable<Space> source,
		TargetCriteria targetCriteria
	) {
		return source
			.SelectMany( x => x.Range( targetCriteria.Range ) )
			.Distinct()
			.Where( s => ctx.Target( s ).Matches( targetCriteria.Filter ) ); // matching this destination
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

#endregion
