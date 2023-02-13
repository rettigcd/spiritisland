namespace SpiritIsland;

/// <summary> Collects tokens from adjacent spaces. aka "Gathers" </summary>
public class TokenGatherer : TokenCollector<TokenGatherer> {

	public TokenGatherer(Spirit self, SpaceState tokens):base(self,tokens) {}

	public Task<SpaceToken[]> GatherN() => Collect( "Gather ", Present.Always );

	public virtual Task<SpaceToken[]> GatherUpToN() => Collect( "Gather up to ", Present.Done );

	protected override IEnumerable<SpaceState> PossibleGatherSources 
		=> _filterSource == null ? _destinationTokens.Adjacent
		: _destinationTokens.Adjacent.Where( _filterSource );

	Func<SpaceState, bool> _filterSource;
	public TokenGatherer FilterSource( Func<SpaceState,bool> filterSource ) {
		_filterSource = filterSource;
		return this;
	}

}

