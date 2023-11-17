namespace SpiritIsland;

public class FollowingPresence : SpiritPresence {
	public FollowingPresence( IPresenceTrack a, IPresenceTrack b, ITokenClass leaderClass ) : base( a, b ) {
		_leaderClass = leaderClass;
	}
	public override void SetSpirit( Spirit spirit ) {
		base.SetSpirit( spirit );
		Token = new FollowingPresenceToken( spirit, _leaderClass ); // replace BEFORE we init the board
	}
	readonly ITokenClass _leaderClass;
}
