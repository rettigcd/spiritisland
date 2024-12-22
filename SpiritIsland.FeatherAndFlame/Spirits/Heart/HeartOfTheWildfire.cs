namespace SpiritIsland.FeatherAndFlame;

public class HeartOfTheWildfire : Spirit {

	public const string Name = "Heart of the Wildfire";

	public override string SpiritName => Name;

	static Track FireAndPlantEnergy => new Track( "fire,plant", Element.Fire, Element.Plant ) {
		Icon = new IconDescriptor {
				BackgroundImg = Img.Coin,
				ContentImg = Img.Token_Fire,
				ContentImg2 = Img.Token_Plant
		}
	};
	public HeartOfTheWildfire() : base( 
		spirit => new SpiritPresence( spirit,
			new PresenceTrack( Track.Energy0, Track.FireEnergy, Track.Energy1, Track.Energy2, FireAndPlantEnergy, Track.Energy3 ),
			new PresenceTrack( Track.Card1, FireCard, Track.Card2, Track.Card3, FireCard, Track.Card4 ),
			new DestructiveNature( spirit )
		),
		new GrowthTrack(
			new GrowthGroup(
				new ReclaimAll(),
				new GainPowerCard(),
				new GainEnergy( 1 )
			),
			new GrowthGroup(
				new GainPowerCard(),
				new PlacePresence( 3 )
			),
			new GrowthGroup(
				new PlacePresence( 1 ),
				new GainEnergy( 2 ),
				new EnergyForFire()
			)
		)
		,PowerCard.ForDecorated(AsphyxiatingSmoke.ActAsync)
		,PowerCard.ForDecorated(FlashFires.ActAsync)
		,PowerCard.ForDecorated(ThreateningFlames.ActAsync)
		,PowerCard.ForDecorated(FlamesFury.ActAsync)
	) {

		InnatePowers = [
			InnatePower.For(typeof(FireStorm)),
			InnatePower.For(typeof(TheBurnedLandRegrows))
		];

		SpecialRules = [
			BlazingPresence_Rule,
			DestructiveNature.Rule
		];
	}

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// in the hightest-numbered Sands on your starting board
		var space = board.Spaces.Last(x => x.IsSand).ScopeSpace;
		// Put 3 presence
		space.Setup(Presence.Token,3);
		// and 2 blight
		space.Blight.Adjust(2); // Blight comes from the box, not the blight card
	}

	static Track FireCard => Track.MkCard( Element.Fire );

	public static SpecialRule BlazingPresence_Rule => new SpecialRule(
		"Blazing Presence",
		"After you add or move presence after Setup, in the land it goes to: For each fire showing on your presence Tracks, do 1 Damage."
		+"  If 2 fire or more are showing on your presence Tracks, add 1 blight."
		+"  Push all beasts and any number of dahan.  Added blight does not destroy your presence."
	);

}
