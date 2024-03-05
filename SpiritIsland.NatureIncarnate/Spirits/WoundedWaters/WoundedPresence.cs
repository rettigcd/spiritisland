namespace SpiritIsland.NatureIncarnate;

public class WoundedPresence : SpiritPresence {

	#region Custom Tracks

	static Track WaterOrAnimal => new Track( "Water/Animal", MultiElements.Build(Element.Water,Element.Animal) ) {
		Icon = new IconDescriptor { ContentImg = Img.Icon_Water, ContentImg2 = Img.Icon_Animal },
	};

	static Track Energy4FirePlant => Track.MkEnergy(4
		,MultiElements.Build(Element.Fire,Element.Plant)
		,new IconDescriptor{ ContentImg = Img.Icon_Fire, ContentImg2 = Img.Icon_Plant }
	);

	static Track Energy5Any => Track.MkEnergy(5, Element.Any );

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
		Energy.TrackRevealedAsync += OnEnergyRevealedAsync;
	}

	async Task OnEnergyRevealedAsync( TrackRevealedArgs args ) {
		int energyRevealed = Energy.Revealed.Count();
		if( energyRevealed <= 4 && CardPlays.Revealed.Count()<energyRevealed )
			await CardPlays.RevealAsync(CardPlays.RevealOptions.First());
	}

	public override IEnumerable<TokenLocation> RevealOptions() { 
		return Energy.Revealed.Count() < 4 
			? Energy.RevealOptions.Select(t=>new TrackPresence(t,Token)) // only show Energy track
			: base.RevealOptions(); // show both tracks
	}

}
