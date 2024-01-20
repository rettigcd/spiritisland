namespace SpiritIsland.Tests.Fear;

public class Quarantine_Tests {

	const string FearAck1 = "Quarantine : 1 : Explore does not affect coastal lands.";
	const string FearAck2 = "Quarantine : 2 : Explore does not affect coastal lands. Lands with disease are not a source of invaders when exploring.";
	const string FearAck3 = "Quarantine : 3 : Explore does not affect coastal lands.  Invaders do not act in lands with disease.";
	readonly IFearCard card = new Quarantine();

	void Init() {
		var powerCard = PowerCard.For(typeof(CallToTend));
		var (userLocal, spirit) = TestSpirit.StartGame( powerCard, (Action<GameState>)(gs => {
			gs.NewLogEntry += ( s ) => { if(s is Log.InvaderActionEntry or Log.RavageEntry) _log.Enqueue( s.Msg() ); };
			gs.InitTestInvaderDeck(
				InvaderCard.Stage1( Terrain.Sands ), // not on coast
				InvaderCard.Stage2Costal(),
				InvaderCard.Stage1( Terrain.Jungle ),
				InvaderCard.Stage1( Terrain.Wetland ) // one extra so we don't trigger 'Time runs out loss'
			);
		}) );
		_user = userLocal;
		_spirit = spirit;
		_log.Clear(); // skip over initial Explorer setup
	}

	[Trait( "Invaders", "Explore" )]
	[Trait( "Invaders", "Deck" )]
	[Theory]
	[InlineData(false)] // 1st card is configed as coastal
	[InlineData(true)]  // A1 A2 A3 are coastland and stopped by Fear
	public void Level1_ExploreDoesNotAffectCoastland( bool activateFearCard ) {
		Init();

		// Given: Activate fear card
		if(activateFearCard)
			_spirit.ActivateFearCard( card );

		GrowAndBuyNoCards();

		if(activateFearCard)
			_user.AcknowledgesFearCard( FearAck1 );

		_user.WaitForNext();
		_log.Assert_Built( "A4", "A7" ); // Sand
		if( activateFearCard )
			_log.Assert_Explored();
		else
			_log.Assert_Explored( "A1","A2","A3" );
	}

	[Trait( "Invaders", "Explore" )]
	[Trait( "Invaders", "Deck" )]
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Level2_ExploreDoesNotAffectCoastlandNorComeFromDiseasedSpots( bool activateFearCard ) {
		Init();

		// Skip over the coastal build
		GrowAndBuyNoCards();

		_user.WaitForNext(); // start of Round 2

		// The only thing around A8 (a jungle) is a diseased town
		_spirit.TargetSpace( "A5" ).Tokens.Given_InitSummary( "" );
		_spirit.TargetSpace( "A6" ).Tokens.Given_InitSummary( "" );
		_spirit.TargetSpace( "A7" ).Tokens.Given_InitSummary( "1T@2,1Z" ); // town & diZease
		_spirit.TargetSpace( "A8" ).Tokens.Given_InitSummary( "" );

		// Given: Activate fear card
		if(activateFearCard) {
			_spirit.ActivateFearCard( card );
			_spirit.ElevateTerrorLevelTo( 2 );
		}
		_log.Clear();

		GrowAndBuyNoCards();

		if(activateFearCard)
			_user.AcknowledgesFearCard( FearAck2 );

		_user.WaitForNext();
		_log.Assert_Ravaged("A4", "A7"); // Sand
		_log.Assert_Built( "A1", "A2", "A3" ); // Costal
		if( activateFearCard )
			_log.Assert_Explored(); // neither A3 (coastal) nor A8 (hanging off of Diseased town) explored
		else
			_log.Assert_Explored( "A3", "A8" );

	}

	[Trait( "Invaders", "Explore" )]
	[Trait( "Invaders", "Deck" )]
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Level3_NoCoastalExplore_NoActionInDiseasedLands( bool activateFearCard ) {
		Init();

		// Skip over the coastal build
		GrowAndBuyNoCards();

		_ = _user.NextDecision; // wait for engine to catch up

		// Ravage lands (sand:A4 & A7) have a disease
		// The only thing around A8 (a jungle) is a diseased town
		_spirit.TargetSpace("A4").Tokens.Given_InitSummary("1E@1,1Z"); // diZease
		_spirit.TargetSpace("A7").Tokens.Given_InitSummary("1E@1,1Z"); // diZease
		// Build lands (Costal:A1..3) all have explorers, A1 has a disease too
		_spirit.TargetSpace("A1").Tokens.Given_InitSummary("1E@1,1Z");
		_spirit.TargetSpace("A2").Tokens.Given_InitSummary("1E@1");
		_spirit.TargetSpace("A3").Tokens.Given_InitSummary("1E@1");
		// Explore lands (jungle:A3 & A8) have a source (A3 is coastal, A8 is town in A5)
		_spirit.TargetSpace("A5").Tokens.Given_InitSummary("1T@2");
		_spirit.TargetSpace("A8").Tokens.Given_InitSummary("1Z");

		// Given: Activate fear card
		if(activateFearCard) {
			_spirit.ActivateFearCard( card );
			_spirit.ElevateTerrorLevelTo(3);
		}

		_log.Clear();
		GrowAndBuyNoCards();

		if(activateFearCard)
			_user.AcknowledgesFearCard( FearAck3 );

		_ = _user.NextDecision; // Wait for invader actions to finish
		if( activateFearCard) {
			_log.Assert_Ravaged();            // Sand (all hvae disease)
			_log.Assert_Built( "A1: build stopped by Quarantine", "A2", "A3" );  // Coastal (A1 has disease)
			_log.Assert_Explored();           // A3 is coastland, A8 has a disease
		} else {
			_log.Assert_Ravaged ("A4", "A7");         // Sand
			_log.Assert_Built   ( "A1", "A2", "A3" ); // Costal
			_log.Assert_Explored( "A3", "A8" );
		}

	}


	[Trait( "Invaders", "Ravage" )]
	[Trait( "Invaders", "Deck" )]
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task SkipRavageWorks( bool skipARavage ) {
		// Not really for quarantine, just a general test without a home

		var spirit = new TestSpirit( PowerCard.For(typeof(CallToTend)) );
		Board board = Board.BuildBoardA();
		GameState gs = new GameState( spirit, board );
		gs.NewLogEntry += ( s ) => { if(s is Log.InvaderActionEntry or Log.RavageEntry) _log.Enqueue( s.Msg() ); };
		gs.InitTestInvaderDeck(
			InvaderCard.Stage1( Terrain.Sands ), // not on coast
			InvaderCard.Stage2Costal(),
			InvaderCard.Stage1( Terrain.Jungle ),
			InvaderCard.Stage1( Terrain.Wetland ) // one extra so we don't trigger 'Time runs out loss'
		);
		gs.Initialize();  // Explore in Sands
		GameState.Current.DisableBlightEffect();

		await InvaderPhase.ActAsync(gs); // Build in Sands, exploring Coastal

		if(skipARavage)
			board[4].Tokens.SkipRavage("Test");

		_log.Clear();
		await InvaderPhase.ActAsync(gs); // Ravage in Sands, Build in Coastal, Explore jungle

		// Then:
		if(skipARavage)
			_log.Assert_Ravaged ( "A7" );             // Sand - A4 skipped
		else
			_log.Assert_Ravaged ( "A4", "A7" );       // Sand

		_log.Assert_Built   ( "A1", "A2", "A3" ); // Costal
		_log.Assert_Explored( "A3", "A8" ); // Jungle
	}



	#region protected / private

	protected VirtualTestUser _user;
	protected Spirit _spirit;
	protected Queue<string> _log = new();

	protected void GrowAndBuyNoCards() {
		_spirit.ClearAllBlight();
		_user.GrowAndBuyNoCards();
	}

	#endregion

}