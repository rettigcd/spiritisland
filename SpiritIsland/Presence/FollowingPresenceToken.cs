using SpiritIsland.Select;

namespace SpiritIsland;


public class FollowingPresenceToken : SpiritPresenceToken {
	readonly IEntityClass _leaderClass;
	public FollowingPresenceToken( Spirit spirit, IEntityClass leaderClass ) : base( spirit ) {
		_leaderClass = leaderClass;
		Text = SpaceAbreviation = "Ts";// to not conflict with Towns
	}
	public override async Task HandleTokenRemovedAsync( ITokenRemovedArgs args ) {
		await base.HandleTokenRemovedAsync( args );
		if(args.Removed.Class == _leaderClass && args is ITokenMovedArgs moved)
			await TryToFollow( moved );
	}

	async Task TryToFollow( ITokenMovedArgs args ) {
		if(!_spirit.Presence.HasMovableTokens( args.From )) return;
		int maxThatCanMove = Math.Min( args.Count, args.From[this] );
		if(maxThatCanMove == 0) return;

		if(1 < maxThatCanMove)
			throw new InvalidOperationException( "Method is only designed to accept 1 move at a time." );

		// Using 'Gather' here so user can click on existing Presence in Source
		// If we used 'Push', user would click on Destination instead of Source
		string prompt = "Move presence with " + args.Removed.Class.Label + "?";
		var source = await _spirit.Gateway.Decision( ASpaceToken.ToCollect( prompt, new SpaceToken[]{ new SpaceToken( args.From.Space, this ) }, Present.Done, args.To.Space ) );
		if(source != null)
			await this.Move( args.From, args.To );
	}

}
