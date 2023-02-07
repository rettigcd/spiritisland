namespace SpiritIsland.JaggedEarth;

public class VengeanceAsABurningPlague : Spirit {

	public const string Name = "Vengeance as a Burning Plague";

	public VengeanceAsABurningPlague() : base(
		new VengeancePresence(
			new PresenceTrack(Track.Energy1,Track.Energy2,Track.AnimalEnergy,Track.Energy3,Track.Energy4),
			new PresenceTrack(Track.Card1, Track.Card2, Track. FireEnergy, Track.Card2, Track.Card3, Track.Card3, Track.Card4)
		)
		,PowerCard.For<FetidBreathSpreadsInfection>()
		,PowerCard.For<FieryVengeance>()
		,PowerCard.For<Plaguebearers>()
		,PowerCard.For<StrikeLowWithSuddenFevers>()
	) {
		GrowthTrack = new GrowthTrack(
			new GrowthOption( new ReclaimAll(), new DrawPowerCard(), new GainEnergy(1)),
			new GrowthOption( new PlacePresence(2,Target.TownOrCity, Target.Blight ), new PlacePresence(2, Target.TownOrCity, Target.Blight ) ),
			new GrowthOption( new DrawPowerCard(), new PlacePresenceOrDisease(), new GainEnergy(1))
		);
		InnatePowers = new InnatePower[] {
			InnatePower.For<EpidemicsRunRampant>(),
			InnatePower.For<SavageRevenge>()
		};
	}

	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[] {  
		TerrorOfASlowlyUnfoldingPlague_Rule,
		LingeringPestilenceToken.Rule,
		WreakVengeanceForTheLandsCorruption.Rule
	};

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// Put 2 presence ontyour starting board:
		// 1 in a land with blight.
		SpaceState landWithoutBlight = gameState.Tokens.PowerUp( board.Spaces ).First( s => s.Blight.Any );
		landWithoutBlight.Adjust( Presence.Token, 1);
		// 1 in a Wetland without dahan
		SpaceState wetlandsWithoutDahan = gameState.Tokens.PowerUp( board.Spaces ).First( s => s.Space.IsWetland && !s.Dahan.Any );
		wetlandsWithoutDahan.Adjust( Presence.Token, 1);

		gameState.Disease_StopBuildBehavior = TerrorOfASlowlyUnfoldingPlague_Handler;

	}

	static public SpecialRule TerrorOfASlowlyUnfoldingPlague_Rule => new SpecialRule(
		"The Terror of a Slowly Unfolding Plague",
		"When disease would prevent a Build on a board with your presence, you may let the Build happen (removing no disease).  If you do, 1 fear."
	);

	// override the behavior Disease uses for this game.
	async Task<bool> TerrorOfASlowlyUnfoldingPlague_Handler( GameCtx gameCtx, SpaceState tokens, TokenClass tokenClass ) {
		bool stoppedByDisease = await this.UserSelectsFirstText( $"Stop pending {tokenClass.Label} build on {tokens.Space.Label}.", "Yes, -1 Disease", "No, +1 Fear, keep Disease " );
		if(stoppedByDisease)
			await tokens.Disease.Remove( 1, RemoveReason.UsedUp );
		else
			gameCtx.GameState.Fear.AddDirect( new FearArgs( 1 ) { space = tokens.Space } );
		return stoppedByDisease;
	}

	public override SelfCtx BindMyPowers( Spirit spirit, GameState gameState ) => new VengenceCtx( spirit );

}
