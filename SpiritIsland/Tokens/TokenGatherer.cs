namespace SpiritIsland;

/// <summary> Collects tokens from adjacent spaces. aka "Gathers" </summary>
public class TokenGatherer : TokenCollector<TokenGatherer> {

	public TokenGatherer(TargetSpaceCtx ctx):base(ctx) {}

	public Task GatherN() => Collect( "Gather ", Present.Always );

	public Task GatherUpToN() => Collect( "Gather up to ", Present.Done );

	protected override IEnumerable<SpaceState> PossibleGatherSources => _destinationCtx.Adjacent;

}

