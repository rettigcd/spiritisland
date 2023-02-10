namespace SpiritIsland.JaggedEarth;

public class VengeancePresence : SpiritPresence {
	public VengeancePresence( PresenceTrack t1, PresenceTrack t2 ) : base( t1, t2 ) {}

	public override void SetSpirit( Spirit spirit ) {
		base.SetSpirit( spirit );
		Token = new LingeringPestilenceToken( spirit ) {
			Destroyed = 1 // 1 of your presence starts the game already Destroyed.
		};
	}
}
