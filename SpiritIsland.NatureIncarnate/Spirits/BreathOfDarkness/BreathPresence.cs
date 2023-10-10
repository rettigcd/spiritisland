namespace SpiritIsland.NatureIncarnate;

public class BreathPresence : IncarnaPresence<BreathIncarna> {

	static Track EmpowerIncarnaTrack => new Track("Empower"){
		Action = new EmpowerIncarna(),
		Icon = new IconDescriptor {  BackgroundImg = Img.BoDDYS_Incarna_Empowered },
	};

	static Track MovePresence => new Track( "Moveonepresence.png" ) {
		Action = new MovePresence( 1 ),
		Icon = new IconDescriptor { BackgroundImg = Img.MovePresence }
	};

	static Track Card4Air => new Track("4 cardplay,air", Element.Air ) {
		Icon = new IconDescriptor {
			BackgroundImg = Img.CardPlay,
			Text = "4",
			Sub = new IconDescriptor { BackgroundImg = Element.Air.GetTokenImg() }
		}
	};

	public BreathPresence() 
		:base(
			new PresenceTrack( Track.Energy1, Track.Energy2, Track.MoonEnergy, Track.Energy3, EmpowerIncarnaTrack, Track.MkEnergy(4,Element.Animal), Track.MkEnergy(5,Element.Air) ),
			new PresenceTrack( Track.Card2, MovePresence, Track.Card3, Track.MoonEnergy, Track.CardReclaim1, Card4Air ),
			new BreathIncarna()
		) {
		}

}
