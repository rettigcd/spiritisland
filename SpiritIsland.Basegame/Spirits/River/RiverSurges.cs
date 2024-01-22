namespace SpiritIsland.Basegame;

/// <summary> River Surges in Sunlight </summary>
public class RiverSurges : Spirit {

	public const string Name = "River Surges in Sunlight";

	public override SpecialRule[] SpecialRules => new SpecialRule[] { new SpecialRule("Rivers Domain", "Your presense in wetlands count as sacred.") };

	public override string Text => Name;

	public RiverSurges():base(
		spirit => new RiverPresence( spirit,
			new PresenceTrack( Track.Energy1, Track.Energy2, Track.Energy2, Track.Energy3, Track.Energy4, Track.Energy4, Track.Energy5 ),
			new PresenceTrack( Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.CardReclaim1, Track.Card4, Track.Card5 )
		),
		new GrowthTrack(
			new GrowthOption(
				new ReclaimAll(),
				new GainPowerCard(),
				new GainEnergy( 1 )
			),
			new GrowthOption(
				new PlacePresence( 1 ),
				new PlacePresence( 1 )
			),
			new GrowthOption(
				new GainPowerCard(),
				new PlacePresence( 2 )
			)
		),
		PowerCard.For(typeof(BoonOfVigor)),
		PowerCard.For(typeof(FlashFloods)),
		PowerCard.For(typeof(RiversBounty)),
		PowerCard.For(typeof(WashAway))
	){
		InnatePowers = [ InnatePower.For(typeof(MassiveFlooding)) ];
	}

	protected override void InitializeInternal( Board board, GameState gs ) {
		board.Spaces.Reverse().First(s => s.IsWetland).Tokens.Setup(Presence.Token, 1);
	}

}
