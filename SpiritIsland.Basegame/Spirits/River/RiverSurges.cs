namespace SpiritIsland.Basegame;

/*
Board:
	A - (S) Great
	C - (A) OK - Can't get to C3 on Round 1 but that is ok because of Flash Flood
	B - (A) OK - Might have to skip B1 to get to B4
	E - (B) Not great - Must choose E4(wetland) or E5
	F - (B) Not great - Can't access F1&F6 with SS until round 2.
	D - (C) Bad - Can't reach town on round 1
*/

/// <summary> River Surges in Sunlight </summary>
public class RiverSurges : Spirit {

	public const string Name = "River Surges in Sunlight";

	public override string SpiritName => Name;

	public RiverSurges():base(
		spirit => new RiversDomain( spirit,
			new PresenceTrack( Track.Energy1, Track.Energy2, Track.Energy2, Track.Energy3, Track.Energy4, Track.Energy4, Track.Energy5 ),
			new PresenceTrack( Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.CardReclaim1, Track.Card4, Track.Card5 )
		),
		new GrowthTrack(
			new GrowthGroup(
				new ReclaimAll(),
				new GainPowerCard(),
				new GainEnergy( 1 )
			),
			new GrowthGroup(
				new PlacePresence( 1 ),
				new PlacePresence( 1 )
			),
			new GrowthGroup(
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
		SpecialRules = [ RiversDomain.Rule ];
	}

	protected override void InitializeInternal( Board board, GameState gs ) {
		board.Spaces.Reverse().First(s => s.IsWetland).ScopeSpace.Setup(Presence.Token, 1);
	}
}
