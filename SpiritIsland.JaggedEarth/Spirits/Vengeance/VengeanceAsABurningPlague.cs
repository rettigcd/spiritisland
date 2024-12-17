namespace SpiritIsland.JaggedEarth;

public class VengeanceAsABurningPlague : Spirit {

	public const string Name = "Vengeance as a Burning Plague";

	public VengeanceAsABurningPlague() : base(
		spirit => new SpiritPresence( spirit,
			new PresenceTrack(Track.Energy1,Track.Energy2,Track.AnimalEnergy,Track.Energy3,Track.Energy4),
			new PresenceTrack(Track.Card1, Track.Card2, Track. FireEnergy, Track.Card2, Track.Card3, Track.Card3, Track.Card4),
			new LingeringPestilencePresenceToken( spirit )
		)
		, new GrowthTrack(
			new GrowthGroup( new ReclaimAll(), new GainPowerCard(), new GainEnergy( 1 ) ),
			new GrowthGroup( new PlacePresence( 2, Filter.Town, Filter.City, Filter.Blight ), new PlacePresence( 2, Filter.Town, Filter.City, Filter.Blight ) ),
			new GrowthGroup( new GainPowerCard(), AddAPresenceOrDisease, new GainEnergy( 1 ) )
		)
		,PowerCard.ForDecorated(FetidBreathSpreadsInfection.ActAsync)
		,PowerCard.ForDecorated(FieryVengeance.ActAsync)
		,PowerCard.ForDecorated(Plaguebearers.ActAsync)
		,PowerCard.ForDecorated(StrikeLowWithSuddenFevers.ActAsync)
	) {
		InnatePowers = [
			InnatePower.For(typeof(EpidemicsRunRampant)), 
			InnatePower.For(typeof(SavageRevenge))
		];
		SpecialRules = [
			TerrorOfASlowlyUnfoldingPlague.Rule,
			LingeringPestilencePresenceToken.Rule,
			WreakVengeanceForTheLandsCorruption.Rule
		];
	}
	static SpiritAction AddAPresenceOrDisease => new SpiritAction(
		"Add a Presence or Disease", 
		self => Cmd.Pick1( AddDiseaseAtRange1, new PlacePresence( 1 ) ).ActAsync( self ) 
	);

	static SpiritAction AddDiseaseAtRange1 => new SpiritAction( "Add a Disease - Range 1", async self => {
		var options = DefaultRangeCalculator.Singleton.GetTargetingRoute_MultiSpace(self.Presence.Lands, new TargetCriteria(1)).Targets;
		Space to = await self.SelectAsync( new A.SpaceDecision( "Add a Disease", options, Present.Always ) );
		await self.Target( to ).Disease.AddAsync( 1 );
	} );


	public override string SpiritName => Name;

	protected override void InitializeInternal( Board board, GameState gameState ) {
		Presence.Destroyed.Count = 1; // 1 of your presence starts the game already Destroyed.

		// Put 2 presence on your starting board:
		// 1 in a land with blight.
		Space landWithoutBlight = board.Spaces.ScopeTokens().First( s => s.Blight.Any );
		landWithoutBlight.Setup( Presence.Token, 1 );

		// 1 in a Wetland without dahan
		Space wetlandsWithoutDahan = board.Spaces.ScopeTokens().First( s => s.SpaceSpec.IsWetland && !s.Dahan.Any );
		wetlandsWithoutDahan.Setup( Presence.Token, 1 );

		gameState.AddIslandMod( new TerrorOfASlowlyUnfoldingPlague(this) );
	}

	public override void InitSpiritAction( ActionScope scope ) {
		if( scope.Category == ActionCategory.Spirit_Power )
			scope.Upgrader = (ss) => new VengeanceSpace( ss );
	}


}
