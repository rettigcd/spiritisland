namespace SpiritIsland.NatureIncarnate;

public class WoundedPresence : SpiritPresence {

	#region Custom Tracks

	static Track WaterOrAnimal => new Track( "Water/Animal" ) {
		Icon = new IconDescriptor { ContentImg = Img.Icon_Water, ContentImg2 = Img.Icon_Animal },
		Action = new PickElement(Element.Water,Element.Animal),
	};
	static Track Energy4FirePlant => new Track( 4 + " energy" ) {
		Energy = 4,
		Icon = new IconDescriptor { BackgroundImg = Img.Coin, Text = 4.ToString(),
			Sub = new IconDescriptor{ ContentImg = Img.Icon_Fire, ContentImg2 = Img.Icon_Plant } },
		Action = new PickElement( Element.Fire, Element.Plant ),
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


	/// <summary> For selecting Water/Animal and Fire/Plant on growth track</summary>
	class PickElement : IActionFactory {
		readonly Element[] _elementOptions;
		public PickElement(params Element[] elementOptions ) {
			_elementOptions = elementOptions;
		}
		public string Name => "Pick " + string.Join(" OR ", _elementOptions);

		string IOption.Text => Name;
		async Task IActionFactory.ActivateAsync( SelfCtx ctx ) {
			var elementToGain = await ctx.Self.SelectElementEx("Gain element", _elementOptions );
			ctx.Self.Elements[elementToGain]++;
		}
		bool IActionFactory.CouldActivateDuring( Phase speed, Spirit spirit ) => true;
	}

	#endregion

	public WoundedPresence():base(
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
