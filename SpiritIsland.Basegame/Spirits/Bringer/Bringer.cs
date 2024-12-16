namespace SpiritIsland.Basegame;

public class Bringer : Spirit {

	public const string Name = "Bringer of Dreams and Nightmares";

	public override string SpiritName => Name;

	public Bringer():base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack( Track.Energy2, Track.AirEnergy, Track.Energy3, Track.MoonEnergy, Track.Energy4, Track.AnyEnergy, Track.Energy5 ),
			new PresenceTrack( Track.Card2, Track.Card2, Track.Card2, Track.Card3, Track.Card3, Track.AnyEnergy )
		)
		, new GrowthTrack(
			// reclaim, +1 power card
			new GrowthGroup( new ReclaimAll(), new GainPowerCard() ),
			// reclaim 1, add presence range 0
			new GrowthGroup( new ReclaimN(), new PlacePresence( 0 ) ),
			// +1 power card, +1 pressence range 1
			new GrowthGroup( new GainPowerCard(), new PlacePresence( 1 ) ),
			// add presense range Dahan or Invadors, +2 energy
			new GrowthGroup( new GainEnergy( 2 ), new PlacePresence( 4, Filter.Dahan, Filter.Invaders ) )
		)
		,PowerCard.ForDecorated(CallOnMidnightsDream.ActAsync)
		,PowerCard.ForDecorated(DreadApparitions.ActAsync)
		,PowerCard.ForDecorated(DreamsOfTheDahan.ActAsync)
		,PowerCard.ForDecorated(PredatoryNightmares.ActAsync)
	) {

		InnatePowers = [
			InnatePower.For(typeof(SpiritsMayYetDream)),
			InnatePower.For(typeof(NightTerrors))
		];

		SpecialRules = [ TDaTD_ActionTokens.Rule ];
	}

	protected override void InitializeInternal( Board board, GameState gs ) {
		// Setup: 2 presense in highest numbered sands
		var startingIn = board.Spaces.Where(x=>x.IsSand).Last();
		var space = startingIn.ScopeSpace;
		space.Setup( Presence.Token, 2 );
	}

	public override void InitSpiritAction( ActionScope scope ) {
		if( scope.Category == ActionCategory.Spirit_Power )
			scope.Upgrader = x => new TDaTD_ActionTokens(x);
	}

}