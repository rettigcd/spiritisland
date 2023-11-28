using SpiritIsland.A;

namespace SpiritIsland.FeatherAndFlame;


public class DownpourDrenchesTheWorld : Spirit, IHaveSecondaryElements {

	public const string Name = "Downpour Drenches The World";
	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[] {DrenchTheLandscape.Rule, PourDownPower.Rule };

	static Track MovePresence => new Track( "Moveonepresence.png" ) {
		Action = new MovePresence( 1 ),
		Icon = new IconDescriptor { BackgroundImg = Img.MovePresence }
	};
	static Track TwoAir => Track.MkEnergy(2,Element.Air);

	static Track TwoWater => new Track("W/W", Element.Water, Element.Water ) {
		Icon = new IconDescriptor {
			ContentImg = Img.Token_Water,
			Sub = new IconDescriptor { BackgroundImg = Img.Token_Water }
		}
	};

	CountDictionary<Element> IHaveSecondaryElements.SecondaryElements
		=> new CountDictionary<Element> { [Element.Water] = pourDownPower.Remaining };

	public DownpourDrenchesTheWorld():base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack(1,Track.Energy1, Track.WaterEnergy, Track.PlantEnergy, Track.WaterEnergy, TwoAir, Track.WaterEnergy, Track.EarthEnergy, TwoWater ),
			new PresenceTrack(1, Track.Card1, MovePresence, Track.WaterEnergy,Track.Card2, MovePresence, Track.Card3 )
		),
		PowerCard.For<DarkSkiesLooseAStingingRain>(),
		PowerCard.For<FoundationsSinkIntoMud>(),
		PowerCard.For<GiftOfAbundance>(),
		PowerCard.For<UnbearableDeluge>()
	) {

		GrowthTrack = new(
			// Reclaim All, Gain Power Card, Move a presence 2 spaces
			new GrowthOption( new ReclaimAll(), new GainPowerCard(), new MovePresence(2) ),
			// Add a Presence(2), Add a Presence(2), Gain 2 water, Discard 2 Power Cards
			new GrowthOption( new PlacePresence(2), new PlacePresence( 2 ), new GainAllElements(Element.Water,Element.Water), new DiscardPowerCards(2) ),
			// Gain Power Card, Add a presence, Gain 1 Energy
			new GrowthOption( new GainPowerCard(), new PlacePresence( 3 ), new GainEnergy(1) )
		);

		InnatePowers = new InnatePower[] {
			InnatePower.For(typeof(RainAndMudSupressConflict)),
			InnatePower.For(typeof(WaterNourishesLifesGrowth))
		};

		pourDownPower = new PourDownPower(this);

	}

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// 1 presence on lowest # wetlands
		board.Spaces.First(x => x.IsWetland).Tokens.Adjust(Presence.Token, 1);
		gameState.TimePasses_WholeGame += _ => { pourDownPower.Reset(); return Task.CompletedTask; };

		gameState.ReplaceTerrain( old => {
			var drenchTheLandscape = new DrenchTheLandscape( this, old );
			return drenchTheLandscape;
		}, ActionCategory.Spirit_Power, ActionCategory.Spirit_Growth, ActionCategory.Spirit_SpecialRule );
	}

	public override IEnumerable<IActionFactory> GetAvailableActions( Phase speed ) {
		return base.GetAvailableActions( speed )
			.Union( pourDownPower.GetAvailableActions( speed ) );
	}

	public override void RemoveFromUnresolvedActions( IActionFactory selectedActionFactory ) {
		if( !pourDownPower.RemoveFromUnresolvedActions( selectedActionFactory ) )
			base.RemoveFromUnresolvedActions(selectedActionFactory);
	}

	protected override object _customSaveValue { 
		get => base._customSaveValue;
		set => pourDownPower.Reset();
	}

	readonly PourDownPower pourDownPower;

}

