namespace SpiritIsland;

/// <summary> Collects tokens from adjacent spaces. aka "Gathers" </summary>
public class TokenGatherer : TokenCollector<TokenGatherer> {

	public TokenGatherer(Spirit self, SpaceState tokens):base(self,tokens) {}

	public Task<SpaceToken[]> GatherN() => Collect( "Gather ", Present.Always );

	public virtual Task<SpaceToken[]> GatherUpToN() => Collect( "Gather up to ", Present.Done );

}

