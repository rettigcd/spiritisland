namespace SpiritIsland.JaggedEarth;

public class GrinningTricksterStirsUpTrouble : Spirit {

	public const string Name = "Grinning Trickster Stirs Up Trouble";
	public override string SpiritName => Name;

	public GrinningTricksterStirsUpTrouble()
		:base(
			x => new SpiritPresence( x,
				new PresenceTrack(Track.Energy1,Track.MoonEnergy,Track.Energy2,Track.AnyEnergy,Track.FireEnergy,Track.Energy3),
				new PresenceTrack(Track.Card2,Track.Push1Dahan,Track.Card3,Track.Card3,Track.Card4,Track.AirEnergy,Track.Card5)
			),
				new GrowthTrack( 2,
				new GrowthGroup( new GainEnergy( -1 ), new ReclaimAll(), new MovePresence( 1 ) ) { GainEnergy = -1 },
				new GrowthGroup( new PlacePresence( 2 ) ),
				new GrowthGroup( new GainPowerCard() ),
				new GrowthGroup( new GainEnergyEqualToCardPlays() )
			)
			,PowerCard.ForDecorated(ImpersonateAuthority.ActAsync)
			,PowerCard.ForDecorated(InciteTheMob.ActAsync)
			,PowerCard.ForDecorated(OverenthusiasticArson.ActAsync)
			,PowerCard.ForDecorated(UnexpectedTigers.ActAsync)
		)
	{

		// Innates
		InnatePowers = [
			InnatePower.For(typeof(LetsSeeWhatHappens)),
			InnatePower.For(typeof(WhyDontYouAndThemFight))
		];

		SpecialRules = [ARealFlairForDiscord.Rule, CleaningUpMessesIsADrag.Rule];
	}

	protected override void InitializeInternal( Board board, GameState gs ) {
		// Place presence on highest numbered land with dahan
		board.Spaces.ScopeTokens().Where( s => s.Dahan.Any ).Last().Setup(Presence.Token, 1);
		// and in land #4
		board[4].ScopeSpace.Setup(Presence.Token, 1);

		// !!! Need an easier way to only apply island-wide mods when is Spirits-own-action
		gs.AddIslandMod( new CleaningUpMessesIsADrag(this) );
		gs.AddIslandMod( new ARealFlairForDiscord(this) );
	}

}
