namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Extends the range that beasts can be pushed.
/// </summary>
class BeastPusher : TokenPusher {

	public BeastPusher( TargetSpaceCtx ctx ) : base( ctx ) { }

	protected override async Task<Space> SelectDestination( IToken token ) {
		int range = token == Token.Beast ? 2 : 1;

		// this is a push, not a range
		IEnumerable<SpaceState> destinationOptions = _ctx.GameState.Tokens[_source].Range( range );

		foreach(var filter in destinationFilters)
			destinationOptions = destinationOptions.Where(filter);

		return await _ctx.Decision( Select.ASpace.PushToken( token, _source, destinationOptions, Present.Always ) );
	}

}