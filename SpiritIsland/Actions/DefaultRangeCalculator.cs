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
		Spirit self, 
		GameState gameState, 
		TargettingFrom powerType,
		IEnumerable<Space> source,
		TargetCriteria targetCriteria
	);

}

public class DefaultRangeCalculator : ICalcRange {

	// Find Range
	// This is virtual so Volcano Targetting can call base()
	public virtual IEnumerable<Space> GetTargetOptionsFromKnownSource( 
		Spirit self, 
		GameState gameState, 
		TargettingFrom powerType,
		IEnumerable<Space> source,
		TargetCriteria targetCriteria
	) {
		var ctx = self.BindMyPower( gameState ); // !!! should this be Power???
		return source       // starting here
			.SelectMany( x => x.Range( targetCriteria.Range ) )
			.Distinct()
			.Where( s => ctx.Target(s).Matches( targetCriteria.Filter ) ); // matching this destination
	}

}

#endregion
