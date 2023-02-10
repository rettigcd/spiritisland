namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Extends the range that beasts can be pushed.
/// </summary>
class BeastPusher : TokenPusher {

	public BeastPusher( TargetSpaceCtx ctx ) : base( ctx ) { }

	protected override async Task<Space> SelectDestination( IToken token ) {
		int range = token == Token.Beast ? 2 : 1;

		IEnumerable<SpaceState> destinationOptions = ctx.GameState.Tokens[source].Range( range ) // this is a push, not a range
			.Where( s => ctx.Target(s.Space).IsInPlay );

		foreach(var filter in destinationFilters)
			destinationOptions = destinationOptions.Where(filter);

		return await ctx.Decision( Select.Space.PushToken( token, source, destinationOptions, Present.Always ) );
	}

}