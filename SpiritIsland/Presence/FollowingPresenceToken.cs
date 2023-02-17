namespace SpiritIsland;

public class FollowingPresenceToken : SpiritPresenceToken {
	readonly IEntityClass _leaderClass;
	public FollowingPresenceToken( Spirit spirit, IEntityClass leaderClass ) : base( spirit ) {
		_leaderClass = leaderClass;
	}
	public override async Task HandleTokenRemovedAsync( ITokenRemovedArgs args ) {
		await base.HandleTokenRemovedAsync( args );
		if(args.Removed.Class == _leaderClass && args is ITokenMovedArgs moved)
			await _spirit.TryToFollow( moved );
	}
}

static public class ExtendSpiritForFollowing {
	static public async Task TryToFollow( this Spirit spirit, ITokenMovedArgs args ) {
		if(!spirit.Presence.HasMovableTokens( args.From )) return;
		int maxThatCanMove = Math.Min( args.Count, args.From[spirit.Token] );
		if(maxThatCanMove == 0) return;

		if(1 < maxThatCanMove)
			throw new InvalidOperationException( "Method is only designed to accept 1 move at a time." );

		// Using 'Gather' here so user can click on existing Presence in Source
		// If we used 'Push', user would click on Destination instead of Source
		var source = await spirit.Gateway.Decision( Select.DeployedPresence.Gather(
			"Move presence with " + args.Removed.Class.Label + "?",
			args.To.Space,
			new SpaceState[] { args.From }, spirit.Token
		) );
		if(source != null)
			// !! This is interesting... This might be a DIFFERENT spirit that is moving the dahan,
			// but WE are calling Bind-MY-Power
			// maybe we should be binding the original spirits power instead of this.
			await spirit.Token.Move( args.From, args.To );
	}

}