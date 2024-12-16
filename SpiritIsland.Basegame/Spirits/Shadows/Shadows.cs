namespace SpiritIsland.Basegame;

public class Shadows : Spirit {

	public const string Name = "Shadows Flicker Like Flame";
	public override string SpiritName => Name;

	public Shadows():base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack( Track.Energy0, Track.Energy1, Track.Energy3, Track.Energy4, Track.Energy5, Track.Energy6 ), 
			new PresenceTrack( Track.Card1, Track.Card2, Track.Card3, Track.Card3, Track.Card4, Track.Card5 )
		),
		new GrowthTrack(
			new GrowthGroup( new ReclaimAll(), new GainPowerCard() ),
			new GrowthGroup( new GainPowerCard(), new PlacePresence( 1 ) ),
			new GrowthGroup( new PlacePresence( 3 ), new GainEnergy( 3 ) )
		),
		PowerCard.ForDecorated(MantleOfDread.ActAsync),
		PowerCard.ForDecorated(FavorsCalledDue.ActAsync),
		PowerCard.ForDecorated(CropsWitherAndFade.ActAsync),
		PowerCard.ForDecorated(ConcealingShadows.ActAsync)
	) {
		InnatePowers = [ InnatePower.For(typeof(DarknessSwallowsTheUnwary)) ];
		SpecialRules = [ShadowsOfTheDahan.Rule];
		Targetter = new ShadowsOfTheDahan(this);
	}

	protected override void InitializeInternal( Board board, GameState gs ) {

		var higestJungle = board.Spaces.Last( s=>s.IsJungle );

		higestJungle.ScopeSpace.Setup(Presence.Token,2 );
		board[5].ScopeSpace.Setup(Presence.Token, 1);
	}

}
