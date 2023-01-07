namespace SpiritIsland.Basegame;

public class Bringer : Spirit {

	public const string Name = "Bringer of Dreams and Nightmares";

	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[] { TDaTD_ActionTokens.Rule };

	public Bringer():base(
		new SpiritPresence(
			new PresenceTrack( Track.Energy2, Track.AirEnergy, Track.Energy3, Track.MoonEnergy, Track.Energy4, Track.AnyEnergy, Track.Energy5 ),
			new PresenceTrack( Track.Card2, Track.Card2, Track.Card2, Track.Card3, Track.Card3, Track.AnyEnergy )
		)
		,PowerCard.For<CallOnMidnightsDream>()
		,PowerCard.For<DreadApparitions>()
		,PowerCard.For<DreamsOfTheDahan>()
		,PowerCard.For<PredatoryNightmares>()
	) {

		GrowthTrack = new(
			// reclaim, +1 power card
			new GrowthOption(new ReclaimAll(),new DrawPowerCard(1)),
			// reclaim 1, add presence range 0
			new GrowthOption(new ReclaimN(), new PlacePresence(0) ),
			// +1 power card, +1 pressence range 1
			new GrowthOption(new DrawPowerCard(1), new PlacePresence(1) ),
			// add presense range Dahan or Invadors, +2 energy
			new GrowthOption(new GainEnergy(2), new PlacePresence(4,Target.Dahan, Target.Invaders ) )
		);

		this.InnatePowers = new InnatePower[]{
			InnatePower.For<SpiritsMayYetDream>(),
			InnatePower.For<NightTerrors>()
		};

	}

	protected override void InitializeInternal( Board board, GameState gs ) {
		// Setup: 2 presense in highest numbered sands
		var startingIn = board.Spaces.Where(x=>x.IsSand).Last();
		var space = gs.Tokens[startingIn];
		Presence.Adjust( space, 2 );

		// Restore Dreamed damaged tokens to original state
		gs.EndOfAction.ForGame.Add( BringerSpaceCtx.CleanupDreamDamage );
	}

	public override SelfCtx BindMyPowers( Spirit spirit, GameState gameState, UnitOfWork actionScope ) 
		=> new BringerCtx( spirit, gameState, actionScope ?? throw new ArgumentNullException(nameof(actionScope) ) );

}