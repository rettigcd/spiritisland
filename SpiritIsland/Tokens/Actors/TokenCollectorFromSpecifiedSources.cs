namespace SpiritIsland;

public class TokenCollectorFromSpecifiedSources : TokenCollector<TokenCollectorFromSpecifiedSources> {

	public TokenCollectorFromSpecifiedSources( TargetSpaceCtx ctx, params SpaceState[] sources ) 
		: base( ctx.Self, ctx.Tokens )
	{
		PossibleGatherSources = sources;
	}
	protected override IEnumerable<SpaceState> PossibleGatherSources { get; }

	/// <remarks>
	/// * Named 'Collect' because that distinguishes it as a collector as opposed to 'Move' which is generic.
	/// * But displays as 'Move' because that is user vernacular.
	/// </remarks>
	public Task<SpaceToken[]> CollectUpToN() => Collect( "Move up to ", Present.Done );
}

