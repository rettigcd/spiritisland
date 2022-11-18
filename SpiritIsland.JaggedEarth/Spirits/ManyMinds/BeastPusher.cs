namespace SpiritIsland.JaggedEarth;

class BeastPusher : TokenPusher {

	public BeastPusher( TargetSpaceCtx ctx ) : base( ctx ) { }

	protected override async Task<Space> SelectDestination( Token token ) {
		int range = token == TokenType.Beast ? 2 : 1;

		IEnumerable<Space> destinationOptions = ctx.GameState.Tokens[source].Range( range )
			.Where( s => ctx.Target(s.Space).IsInPlay )
			.Select( s => s.Space );

		foreach(var filter in destinationFilters)
			destinationOptions = destinationOptions.Where(filter);

		return await ctx.Decision( Select.Space.PushToken( token, source, destinationOptions, Present.Always ) );
	}

}