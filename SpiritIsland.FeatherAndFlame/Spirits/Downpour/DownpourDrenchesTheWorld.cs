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
		=> new CountDictionary<Element> { [Element.Water] = _pourDownPower.Remaining };

	public DownpourDrenchesTheWorld():base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack(1,Track.Energy1, Track.WaterEnergy, Track.PlantEnergy, Track.WaterEnergy, TwoAir, Track.WaterEnergy, Track.EarthEnergy, TwoWater ),
			new PresenceTrack(1, Track.Card1, MovePresence, Track.WaterEnergy,Track.Card2, MovePresence, Track.Card3 )
		),
		new GrowthTrack(
			// Reclaim All, Gain Power Card, Move a presence 2 spaces
			new GrowthGroup( new ReclaimAll(), new GainPowerCard(), new MovePresence( 2 ) ),
			// Add a Presence(2), Add a Presence(2), Gain 2 water, Discard 2 Power Cards
			new GrowthGroup( new PlacePresence( 2 ), new PlacePresence( 2 ), new GainAllElements( Element.Water, Element.Water ), new DiscardCards( 2 ) ),
			// Gain Power Card, Add a presence, Gain 1 Energy
			new GrowthGroup( new GainPowerCard(), new PlacePresence( 3 ), new GainEnergy( 1 ) )
		),
		PowerCard.For(typeof(DarkSkiesLooseAStingingRain))
		,PowerCard.For(typeof(FoundationsSinkIntoMud))
		,PowerCard.For(typeof(GiftOfAbundance))
		,PowerCard.For(typeof(UnbearableDeluge))
	) {
		InnatePowers = [
			InnatePower.For(typeof(RainAndMudSupressConflict)),
			InnatePower.For(typeof(WaterNourishesLifesGrowth))
		];

		_pourDownPower = new PourDownPower(this);

	}

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// 1 presence on lowest # wetlands
		board.Spaces.First(x => x.IsWetland).Tokens.Setup(Presence.Token, 1);
		gameState.AddTimePassesAction( _pourDownPower );

		gameState.ReplaceTerrain( old => {
			var drenchTheLandscape = new DrenchTheLandscape( this, old );
			return drenchTheLandscape;
		}, ActionCategory.Spirit_Power, ActionCategory.Spirit_Growth, ActionCategory.Spirit_SpecialRule );
	}

	public override IEnumerable<IActionFactory> GetAvailableActions( Phase speed ) {
		return base.GetAvailableActions( speed )
			.Union( _pourDownPower.GetAvailableActions( speed ) );
	}

	public override void RemoveFromUnresolvedActions( IActionFactory selectedActionFactory ) {
		if( !_pourDownPower.RemoveFromUnresolvedActions( selectedActionFactory ) )
			base.RemoveFromUnresolvedActions(selectedActionFactory);
	}

	protected override object CustomMementoValue { 
		get => base.CustomMementoValue;
		set => _pourDownPower.Reset();
	}

	readonly PourDownPower _pourDownPower;

}

