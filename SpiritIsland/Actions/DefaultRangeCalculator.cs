namespace SpiritIsland;

#region Source
public enum From { None, Presence, SacredSite };

public interface ICalcSource {
	IEnumerable<Space> FindSources( IKnowSpiritLocations presence, TargetSourceCriteria source );

}

public class DefaultSourceCalc : ICalcSource {
	public virtual IEnumerable<Space> FindSources( IKnowSpiritLocations presence, TargetSourceCriteria source ) {
		var sources = source.From switch {
			From.Presence => presence.Spaces,
			From.SacredSite => presence.SacredSites,
			_ => throw new ArgumentException( "Invalid presence source " + source.From ),
		};
		return sources.Where( x => !source.Terrain.HasValue || x.Is( source.Terrain.Value ) );
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
		var ctx = self.Bind( gameState, Cause.Power );
		return source       // starting here
			.SelectMany( x => x.Range( targetCriteria.Range ) )
			.Distinct()
			.Where( s => ctx.Target(s).Matches( targetCriteria.Filter ) ); // matching this destination
	}

}

#endregion
