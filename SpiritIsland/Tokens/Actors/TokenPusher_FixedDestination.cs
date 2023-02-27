namespace SpiritIsland;

/// <summary> Overrides Selecting destination with a fixed destination </summary>
public class TokenPusher_FixedDestination : TokenPusher {
	readonly Space destination;
	public TokenPusher_FixedDestination( TargetSpaceCtx ctx, Space destination ) : base( ctx.Self, ctx.Tokens ) { 
		this.destination = destination;
	}

	protected override Task<Space> SelectDestination( IToken token ) {
		return Task.FromResult(destination);
	}

}
