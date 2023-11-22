namespace SpiritIsland.NatureIncarnate;

public class WoundedPresence : SpiritPresence {

	#region Custom Tracks

	static Track WaterOrAnimal => new Track( "Water/Animal", MultiElements.Build(Element.Water,Element.Animal) ) {
		Icon = new IconDescriptor { ContentImg = Img.Icon_Water, ContentImg2 = Img.Icon_Animal },
//		Action = new Gain1Element(Element.Water,Element.Animal),
	};
	static Track Energy4FirePlant => new Track( 4 + " energy" ) {
		Energy = 4,
		Icon = new IconDescriptor { BackgroundImg = Img.Coin, Text = 4.ToString(),
			Sub = new IconDescriptor{ ContentImg = Img.Icon_Fire, ContentImg2 = Img.Icon_Plant } },
		Action = new Gain1Element( Element.Fire, Element.Plant ),
	};
	static Track Energy5Any => new Track( 5 + " energy" ) {
		Energy = 5,
		Elements = new Element[] { Element.Any },
		Icon = new IconDescriptor {
			BackgroundImg = Img.Coin,
			Text = 5.ToString(),
			Sub = new IconDescriptor { ContentImg = Img.Token_Any }
		},
	};

	static Track Gather1Blight => new Track( "GatherBlight" ) {
		Icon = new IconDescriptor {
			ContentImg = Img.Land_Gather_Blight,
		},
		Action = new Gather1Token(0,SpiritIsland.Token.Blight),
	};

	#endregion

	public WoundedPresence(Spirit spirit):base(spirit,
		new PresenceTrack( 1, Track.Energy0, WaterOrAnimal, Gather1Blight, Track.Energy1, Track.Energy3, Energy4FirePlant,   Energy5Any ),
		new PresenceTrack( 1, Track.Card1,   Track.Card1,   Track.Card1,   Track.Card2,   Track.Card3,   Track.CardReclaim1, Track.Card4   )
	) {
		Energy.TrackRevealed.Add( OnRevealed );
	}

	Task OnRevealed( TrackRevealedArgs args ) {
		int energyRevealed = Energy.Revealed.Count();
		if( energyRevealed <= 4 && CardPlays.Revealed.Count()<energyRevealed )
			CardPlays.Reveal(CardPlays.RevealOptions.First());
		return Task.CompletedTask;
	}

	public override IEnumerable<Track> RevealOptions() { 
		return Energy.Revealed.Count() < 4 ? Energy.RevealOptions : base.RevealOptions();
	}

}
