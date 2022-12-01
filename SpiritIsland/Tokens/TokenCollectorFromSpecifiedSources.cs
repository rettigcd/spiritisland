namespace SpiritIsland;

public class TokenCollectorFromSpecifiedSources : TokenCollector<TokenCollectorFromSpecifiedSources> {

	public TokenCollectorFromSpecifiedSources( TargetSpaceCtx destinationCtx, params SpaceState[] sources ) : base( destinationCtx ) {
		PossibleGatherSources = sources;
	}
	protected override IEnumerable<SpaceState> PossibleGatherSources { get; }

	/// <remarks>
	/// These 2 methods are:
	/// * Named 'Collect' because that distinguishes it as a collector as opposed to 'Move' which is generic.
	/// * But displays as 'Move' because that is user vernacular.
	/// </remarks>
	public Task CollectN() => Collect( "Move ", Present.Always );
	public Task CollectUpToN() => Collect( "Move up to ", Present.Done );
}

