namespace SpiritIsland;

/// <summary>
/// 
/// </summary>
/// <param name="Restrict">Comma-Delimited Target string constancts</param>
public record TargetingSourceCriteria( TargetFrom From, string Restrict = null ) {

	public IEnumerable<SpaceState> Filter( IEnumerable<SpaceState> sources ) { 
		if( Restrict != null)
			sources = sources.Where( new SpaceCriteria( null, Restrict.Split(',') ).Matches );
		return sources;
	}

	public IEnumerable<SpaceState> GetSources( Spirit spirit )
		=> Filter( spirit.TargetingSourceStrategy.EvaluateFrom( spirit.Presence, From ) ).ToArray();


}
