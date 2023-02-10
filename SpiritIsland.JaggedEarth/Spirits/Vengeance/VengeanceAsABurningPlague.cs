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
		TerrorOfASlowlyUnfoldingPlague.Rule,
		LingeringPestilenceToken.Rule,
		WreakVengeanceForTheLandsCorruption.Rule
	};

	protected override void InitializeInternal( Board board, GameState gameState ) {
		// Put 2 presence on your starting board:
		// 1 in a land with blight.
		SpaceState landWithoutBlight = board.Spaces.Upgrade().First( s => s.Blight.Any );
		landWithoutBlight.Adjust( Presence.Token, 1);
		// 1 in a Wetland without dahan
		SpaceState wetlandsWithoutDahan = board.Spaces.Upgrade().First( s => s.Space.IsWetland && !s.Dahan.Any );
		wetlandsWithoutDahan.Adjust( Presence.Token, 1);

		// Swap out old Disease with new.
		var newDisease = new TerrorOfASlowlyUnfoldingPlague( this );
		gameState.Tokens.TokenDefaults[SpiritIsland.Token.Disease] = newDisease;
		foreach(SpaceState space in gameState.AllSpaces)
			space.ReplaceAllWith(SpiritIsland.Token.Disease_Original,newDisease);
	}

	public override SelfCtx BindMyPowers( Spirit spirit, GameState gameState ) => new VengenceCtx( spirit );

}
