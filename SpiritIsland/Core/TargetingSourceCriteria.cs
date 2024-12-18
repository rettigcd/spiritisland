#nullable enable
namespace SpiritIsland;

/// <param name="Restrict">Comma-Delimited Target string constancts</param>
public record TargetingSourceCriteria( TargetFrom From, string? Restrict = null ) {

	public IEnumerable<Space> GetSources(Spirit spirit)
		=> Filter(spirit.TargetingSourceStrategy.EvaluateFrom(spirit.Presence, From)).ToArray();

	/// <remarks>public so Mists Shift and flow can use it.</remarks>
	public IEnumerable<Space> Filter( IEnumerable<Space> sources ) { 
		if( Restrict != null)
			sources = sources.Where( new SpaceCriteria( null, Restrict.Split(',') ).Matches );
		return sources;
	}

}
