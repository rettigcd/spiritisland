namespace SpiritIsland.NatureIncarnate;

public class HearthPresence : SpiritPresence {

	static Track Energy0_GatherDahanBonus => new Track( "energy0" ) {
		Icon = new IconDescriptor {
			BackgroundImg = Img.Coin,
			Text = "0",
		},
		Action = new Gather1Dahan()
	};

	public HearthPresence() : base( 
		new PresenceTrack( Energy0_GatherDahanBonus, Track.MkEnergy( 1, Element.Sun ), Track.Energy2, Track.MkEnergy( 3, Element.Animal ), Track.Energy4, Track.MkEnergy( 5, Element.Sun ) ),
		new PresenceTrack( Track.Card1, Track.Card2, Track.AirEnergy, Track.Card3, Track.AnimalEnergy, Track.Card4 )
	) { }

	public override void SetSpirit( Spirit spirit ) {
		this.Token = new HearthToken( spirit );
	}

}
