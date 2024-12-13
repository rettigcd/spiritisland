namespace SpiritIsland;


/// <summary> A presence token that follows other tokens around. (Thunderspeaker & Sharp Fangs) </summary>
public class FollowingPresenceToken : SpiritPresenceToken {
	readonly ITokenClass _leaderClass;
	public FollowingPresenceToken( Spirit spirit, ITokenClass leaderClass ) : base( spirit ) {
		_leaderClass = leaderClass;
		SpaceAbreviation = "Ts";// to not conflict with Towns
	}
	public override async Task HandleTokenRemovedAsync(ITokenRemovedArgs args ) {
		await base.HandleTokenRemovedAsync( args );
		await TrackLeaderToken( args );
	}

	async Task TrackLeaderToken( ITokenRemovedArgs args ) {
		if(args.Removed.Class == _leaderClass && args is ITokenMovedArgs moved)
			await TryToFollow( (Space)args.From, moved );
	}

	async Task TryToFollow( Space from, ITokenMovedArgs args ) {
		if(!from.Has(Self.Presence)) return;
		if(args.To is not Space to) return; // can only follow to spaces, not presence track/destroyed
		int maxThatCanMove = Math.Min( MaxFollowerCount( args ), from[this] );
		if(maxThatCanMove == 0) return;

		// Using 'Gather' here so user can click on existing Presence in Source
		// If we used 'Push', user would click on Destination instead of Source
		string prompt = "Move presence with " + args.Removed.Class.Label + "?";
		while(0 < maxThatCanMove--) {
			var source = await Self.SelectAsync( A.SpaceTokenDecision.ToCollect( prompt, new SpaceToken[] { this.On( from ) }, Present.Done, to.SpaceSpec ) );
			if(source != null)
				await this.MoveAsync(from,to);
		}
	}

	protected virtual int MaxFollowerCount( ITokenMovedArgs args ) => args.Count;
}
