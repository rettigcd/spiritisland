namespace SpiritIsland.NatureIncarnate;

public class HearthPresence : SpiritPresence {

	static Track Energy0_GatherDahanBonus => new Track( "energy0" ) {
		Icon = new IconDescriptor {
			BackgroundImg = Img.Coin,
			Text = "0",
		},
		Action = new Gather1Dahan()
	};

	class Gather1Dahan : IActionFactory {
		public string Name => "Gather up to 3 dahan into one of your lands.";
		public string Text => Name;
		public bool CouldActivateDuring( Phase speed, Spirit spirit ) => true;	// !!! is this ever used?
		public Task ActivateAsync( SelfCtx ctx ) 
			=> Cmd.GatherUpToNDahan( 1 )
				.To().SpiritPickedLand().Which( Has.YourPresence )
				.Execute( ctx );
	}

	public HearthPresence() : base( 
		new PresenceTrack( Energy0_GatherDahanBonus, Track.MkEnergy( 1, Element.Sun ), Track.Energy2, Track.MkEnergy( 3, Element.Animal ), Track.Energy4, Track.MkEnergy( 5, Element.Sun ) ),
		new PresenceTrack( Track.Card1, Track.Card2, Track.AirEnergy, Track.Card3, Track.AnimalEnergy, Track.Card4 )
	) { }

	public override void SetSpirit( Spirit spirit ) {
		this.Token = new HearthToken( spirit );
	}

}
