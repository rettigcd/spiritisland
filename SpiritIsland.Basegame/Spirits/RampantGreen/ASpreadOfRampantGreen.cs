namespace SpiritIsland.Basegame;

public class ASpreadOfRampantGreen : Spirit {

	public const string Name = "A Spread of Rampant Green";

	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[] {
		ChokeTheLandWithGreen.Rule,
		SteadyRegeneration
	};

	static SpecialRule SteadyRegeneration => new SpecialRule(
		"Steady Regeneration",
		"When adding Presence to the board via Growth, you may optionally use your destroyed Presence. If the island is Healthy, do so freely. If the island is Blighted, doing so costs 1 Energy per destroyed Presence you add."
	);

	public ASpreadOfRampantGreen():base(
		new RampantGreenPresence(),
		PowerCard.For<FieldsChokedWithGrowth>(),
		PowerCard.For<GiftOfProliferation>(),
		PowerCard.For<OvergrowInANight>(),
		PowerCard.For<StemTheFlowOfFreshWater>()
	) {
		// Special rules: steady regeneration

		GrowthTrack = new GrowthTrack(
			new GrowthOption( new PlacePresence( 2, Target.Jungle, Target.Wetland ) )
		).Add( new GrowthPickGroups( 1,
			// reclaim, +1 power card
			new GrowthOption(
				new ReclaimAll(), 
				new DrawPowerCard(1)
			),
			// +1 presense range 1, play +1 extra card this turn
			new GrowthOption(
				new PlacePresence(1),
				new PlayExtraCardThisTurn(1)
			),
			// +1 power card, +3 energy
			new GrowthOption(
				new GainEnergy(3), 
				new DrawPowerCard()
			)
		));

		this.InnatePowers = new InnatePower[] {
			InnatePower.For<CreepersTearIntoMortar>(),
			InnatePower.For<AllEnvelopingGreen>(),
		};

	}

	protected override void InitializeInternal( Board board, GameState gs ) {

		// Setup: 1 in the highest numbered wetland 
		board.Spaces.Reverse().First( x => x.IsWetland ).Tokens.Adjust( Presence.Token, 1 );
		// and 1 in the jungle without any dahan
		board.Spaces.Single( x => x.IsJungle && x.Tokens.Dahan.CountAll == 0 ).Tokens.Adjust( Presence.Token, 1 );

	}

}
