namespace SpiritIsland;

/// <summary> A presence token that follows other tokens around. (Thunderspeaker & Sharp Fangs) </summary>
public class FollowingPresenceToken : SpiritPresenceToken {

	readonly ITokenClass _leaderClass;

	public FollowingPresenceToken( Spirit spirit, ITokenClass leaderClass, string spaceAbreviation ) : base( spirit ) {
		_leaderClass = leaderClass;
		SpaceAbreviation = spaceAbreviation;// to not conflict with Towns
	}

	public override async Task HandleTokenRemovedAsync(ITokenRemovedArgs args ) {
		await base.HandleTokenRemovedAsync( args );
		await TrackLeaderToken( args );
	}

	async Task TrackLeaderToken( ITokenRemovedArgs args ) {
		if( args is ITokenMovedArgs moved ) 
			if( args.Removed.Class == _leaderClass)
				await TryToFollowLeader( moved );
	}

	async Task TryToFollowLeader( ITokenMovedArgs args ) {
		// can only follow between spaces, not presence track/destroyed
		if( args.From is not Space from || args.To is not Space to ) return;

		int followerCount = from.Sum(Self.Presence); // includes both Normal presence + Incarna
		int maxThatCanMove = Math.Min( args.Count, followerCount );
		if(maxThatCanMove == 0) return;

		FollowedMoves.Add(args);

		// Using 'Gather' here so user can click on existing Presence in Source
		// If we used 'Push', user would click on Destination instead of Source
		string prompt = "Move presence with " + args.Removed.Class.Label + "?";
		var spaceTokens = from.OfTag(Self.Presence).On( from );
		while(0 < maxThatCanMove--) {
			SpaceToken? source = await Self.Select( A.SpaceTokenDecision.ToCollect( prompt, spaceTokens, Present.Done, to.SpaceSpec ) );
			if(source is not null)
				await source.MoveTo(to);
		}
	}

	public List<ITokenMovedArgs> FollowedMoves => ActionScope.Current.SafeGet<List<ITokenMovedArgs>>(Key, () => []);
	string Key => "FollowedMoves-" + SpaceAbreviation;
}
