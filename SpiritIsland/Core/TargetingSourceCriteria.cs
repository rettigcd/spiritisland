namespace SpiritIsland;

public record TargetingSourceCriteria( From From, string Restrict = null ) {

	public IEnumerable<SpaceState> Filter( IEnumerable<SpaceState> sources ) { 
		if( Restrict != null)
			sources = sources.Where( new SpaceCriteria( null, Restrict ).Matches );
		return sources;
	}

	public IEnumerable<SpaceState> GetSources( Spirit spirit )
		=> Filter( spirit.TargetingSourceStrategy.EvaluateFrom( spirit.Presence, From ) ).ToArray();


}
