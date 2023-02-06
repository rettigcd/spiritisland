namespace SpiritIsland.JaggedEarth;

class ManyMindsPresence : SpiritPresence {

	public ManyMindsPresence(PresenceTrack energy, PresenceTrack cards ) : base( energy, cards ) {
		Token = new ManyMindsToken();
	}

}
