
namespace SpiritIsland.Basegame;

public class VitalStrength : Spirit {

	public const string Name = "Vital Strength of the Earth";
	public override string SpiritName => Name;

	public VitalStrength():base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack( Track.Energy2, Track.Energy3, Track.Energy4, Track.Energy6, Track.Energy7, Track.Energy8 ),
			new PresenceTrack( Track.Card1, Track.Card1, Track.Card2, Track.Card2, Track.Card3, Track.Card4 ),
			new EarthsVitality(spirit)
		),
		new GrowthTrack(
			new GrowthGroup( new ReclaimAll(), new PlacePresence( 2 ) ),
			new GrowthGroup( new GainPowerCard(), new PlacePresence( 0 ) ),
			new GrowthGroup( new GainEnergy( 2 ), new PlacePresence( 1 ) )
		),
		PowerCard.ForDecorated(GuardTheHealingLand.ActAsync),
		PowerCard.ForDecorated(AYearOfPerfectStillness.ActAsync),
		PowerCard.ForDecorated(RitualsOfDestruction.ActAsync),
		PowerCard.ForDecorated(DrawOfTheFruitfulEarth.ActAsync)
	){
		InnatePowers = [ InnatePower.For(typeof(GiftOfStrength)) ];
		SpecialRules = [ EarthsVitality.Rule ];
	}

	protected override void InitializeInternal( Board board, GameState gs ) {
		InitPresence( board );
	}

	void InitPresence( Board board ){
		var higestJungle = board.Spaces.OrderByDescending( s => s.Label ).First( s => s.IsJungle );
		var higestMountain = board.Spaces.OrderByDescending(s => s.Label).First(s => s.IsMountain);
		higestMountain.ScopeSpace.Setup( Presence.Token, 2 );
		higestJungle.ScopeSpace.Setup( Presence.Token, 1 );
	}

}
