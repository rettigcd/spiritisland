namespace SpiritIsland.Basegame;

public class Bringer : Spirit {

	public const string Name = "Bringer of Dreams and Nightmares";

	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[] { TDaTD_ActionTokens.Rule };

	public Bringer():base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack( Track.Energy2, Track.AirEnergy, Track.Energy3, Track.MoonEnergy, Track.Energy4, Track.AnyEnergy, Track.Energy5 ),
			new PresenceTrack( Track.Card2, Track.Card2, Track.Card2, Track.Card3, Track.Card3, Track.AnyEnergy )
		)
		, new GrowthTrack(
			// reclaim, +1 power card
			new GrowthOption( new ReclaimAll(), new GainPowerCard() ),
			// reclaim 1, add presence range 0
			new GrowthOption( new ReclaimN(), new PlacePresence( 0 ) ),
			// +1 power card, +1 pressence range 1
			new GrowthOption( new GainPowerCard(), new PlacePresence( 1 ) ),
			// add presense range Dahan or Invadors, +2 energy
			new GrowthOption( new GainEnergy( 2 ), new PlacePresence( 4, Filter.Dahan, Filter.Invaders ) )
		)
		, PowerCard.For(typeof(CallOnMidnightsDream))
		,PowerCard.For(typeof(DreadApparitions))
		,PowerCard.For(typeof(DreamsOfTheDahan))
		,PowerCard.For(typeof(PredatoryNightmares))
	) {

		InnatePowers = new InnatePower[]{
			InnatePower.For(typeof(SpiritsMayYetDream)),
			InnatePower.For(typeof(NightTerrors))
		};

	}

	protected override void InitializeInternal( Board board, GameState gs ) {
		// Setup: 2 presense in highest numbered sands
		var startingIn = board.Spaces.Where(x=>x.IsSand).Last();
		var space = startingIn.Tokens;
		space.Adjust( Presence.Token, 2 );
	}

	public override void InitSpiritAction( ActionScope scope ) {
		if( scope.Category == ActionCategory.Spirit_Power )
			scope.Upgrader = x => new TDaTD_ActionTokens(x);
	}

}