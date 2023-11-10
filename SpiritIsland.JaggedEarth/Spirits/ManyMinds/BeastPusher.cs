namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Extends the range that beasts can be pushed.
/// </summary>
class BeastPusher : TokenPusher {

	public BeastPusher( Spirit self, SpaceState tokens ) : base( self, tokens ) { }

	protected override async Task<Space> SelectDestination( IToken token ) {
		int range = token.Class == Token.Beast ? 2 : 1; // Compare Class, not Token so we get all beasts

		// this is a push, not a range
		IEnumerable<SpaceState> destinationOptions = _tokens.Range( range );

		foreach(var filter in destinationFilters)
			destinationOptions = destinationOptions.Where(filter);

		return await _self.Gateway.Select( A.Space.ToPushToken( token, _tokens.Space, destinationOptions, Present.Always ) );
	}

}