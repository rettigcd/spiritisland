namespace SpiritIsland;


/// <summary> A presence token that follows other tokens around. (Thunderspeaker & Sharp Fangs) </summary>
public class FollowingPresenceToken : SpiritPresenceToken {
	readonly IEntityClass _leaderClass;
	public FollowingPresenceToken( Spirit spirit, IEntityClass leaderClass ) : base( spirit ) {
		_leaderClass = leaderClass;
		Text = SpaceAbreviation = "Ts";// to not conflict with Towns
	}
	public override async Task HandleTokenRemovedAsync( ITokenRemovedArgs args ) {
		await base.HandleTokenRemovedAsync( args );
		await TrackLeaderToken( args );
	}

	async Task TrackLeaderToken( ITokenRemovedArgs args ) {
		if(args.Removed.Class == _leaderClass && args is ITokenMovedArgs moved)
			await TryToFollow( moved );
	}

	async Task TryToFollow( ITokenMovedArgs args ) {
		if(!_spirit.Presence.HasMovableTokens( args.From )) return;
		int maxThatCanMove = Math.Min( MaxFollowerCount( args ), args.From[this] );
		if(maxThatCanMove == 0) return;

		// Using 'Gather' here so user can click on existing Presence in Source
		// If we used 'Push', user would click on Destination instead of Source
		string prompt = "Move presence with " + args.Removed.Class.Label + "?";
		while(0 < maxThatCanMove--) {
			var source = await _spirit.Gateway.Select( A.SpaceToken.ToCollect( prompt, new SpaceToken[] { new SpaceToken( args.From.Space, this ) }, Present.Done, args.To.Space ) );
			if(source != null)
				await this.Move( args.From, args.To );
		}
	}

	protected virtual int MaxFollowerCount( ITokenMovedArgs args ) => args.Count;
}
