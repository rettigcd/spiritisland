namespace SpiritIsland.JaggedEarth;

class ManyMindsPresence : SpiritPresence {

	public ManyMindsPresence(PresenceTrack energy, PresenceTrack cards ) : base( energy, cards ) {
	}

	public override void SetSpirit( Spirit spirit ) { 
		base.SetSpirit( spirit );
		Token = new ManyMindsToken( spirit );
	}

}
