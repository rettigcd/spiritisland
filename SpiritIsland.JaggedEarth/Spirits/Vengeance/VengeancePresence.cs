namespace SpiritIsland.JaggedEarth;

public class VengeancePresence : SpiritPresence {
	public VengeancePresence(PresenceTrack t1, PresenceTrack t2 ) : base( t1, t2 ) {
		// 1 of your presence starts the game already Destroyed.
		Destroyed = 1;

		DestroyBehavior = new LingeringPestilence();
	}

}