namespace SpiritIsland.Tests.Fear;

public class TradeSuffers_Tests {

	void Init() {
		// On Board-A,
		// use A7 (Sands-2 Dahan)
		// or A4 (Sands-no dahan)
		var (user, spirit, _) = TestSpirit.StartGame( PowerCard.For(typeof(RiversBounty)), gs => {
			var fear = gs.Fear;
			InitMountainThenAllSands( gs );
			gs.NewLogEntry += ( s ) => _log.Add( s.Msg() );
		} );
	}

	[Fact]
	[Trait( "Invaders", "Build" )]
	public async Task Level1_CityPresent_NoBuild() {

		var gs = new GameState(new RiverSurges(), Boards.B);
		var b5 = gs.Island.Boards[0][5].ScopeSpace;

		// Given: City on Space stops build
		b5.Given_InitSummary("1C@3");

		//  When: Trade Suffers - Level 1
		await new TradeSuffers().ActAsync(1);

		//   And: builds
		await b5.When_CardBuilds();

		//  Then: no town is built
		b5.Summary.ShouldBe("1C@3");

	}

	[Fact]
	[Trait("Invaders", "Build")]
	public async Task Level1_CityAddedAfterFear_NoBuild() {

		// Tests that city is evaluated at Build Time, not at Fear time.

		var gs = new GameState(new RiverSurges(), Boards.B);
		var b5 = gs.Island.Boards[0][5].ScopeSpace;

		// Given: No City on Space
		b5.InitDefault(Human.City,0);

		//   And: Trade Suffers - Level 1
		await new TradeSuffers().ActAsync(1);

		//  When: city is added after the fact.  (Like from a Blighted island during Ravage)
		b5.InitDefault(Human.City, 1);

		//   And: builds
		await b5.When_CardBuilds();

		//  Then: no town is built
		b5.Summary.ShouldBe("1C@3");

	}

	[Fact]
	public async Task Level1_CityDestroyedDuringRavage_Build() {
		// On Board-A,
		// use A7 (Sands-2 Dahan)
		// or A4 (Sands-no dahan)
		TestSpirit spirit = new TestSpirit( PowerCard.For(typeof(RiversBounty)) );
		var user = new VirtualTestUser( spirit );
		Board board = Board.BuildBoardA();
		GameState gs = new GameState( spirit, board ) {
			InvaderDeck = InvaderDeckBuilder.Default.Build() // Same order every time
		};
		InitMountainThenAllSands( gs );
		gs.NewLogEntry += ( s ) => _log.Add( s.Msg() );
		gs.Initialize(); 

		// Disable destroying presence
		gs.DisableBlightEffect();

		// Round 1
		await InvaderPhase.ActAsync(gs);
		ClearBlight();

		// Round 2
		await InvaderPhase.ActAsync(gs);
		ClearBlight();

		// Round 3
		//  And: Fear card is active and ready to flip
		await new TradeSuffers().ActAsync(1);


		// Given: 1 city and a enough dahan to kill the city but not the last explorer
		Space space = board[7].ScopeSpace; // _ctx.TargetSpace( "A7" ).Tokens;
		space.Given_InitSummary( "1C@3,4D@2,2E@1" );

		// When: activating Fear & Doing Invader Actions
		await InvaderPhase.ActAsync(gs).ShouldComplete();
		await gs.TriggerTimePasses(); // let them heal

		// Ravage-a: 1 city + 2 explorers do 5 damage killing 2 dahan    1B@1,1C@1,2D@2,2E@1
		// Ravage-b: 2 dahan do 4 damage killing city and 1 explorer     1B@1,2D@2,1E@1 
		// Build: no city present => build		1B@1,2D@2,1E@1,1T@2
		// Explore: +1							1B@1,2D@2,2E@1,1T@2
		space.Summary.ShouldBe( "1B,2D@2,2E@1,1T@2" );

	}

	[Fact]
	public async Task Level3_DoesntForceCityReplace() {
		Init();

		// "Each player may replace 1 City with 1 Town or 1 Town with 1 Explorer in a Coastal land." )]
		var fxt = new ConfigurableTestFixture();

		// Given: A1 has a city
		var board = fxt.GameState.Island.Boards[0];
		var space = board[1];
		fxt.InitTokens( space, "1C@3" );

		// When: fear card
		await new TradeSuffers().When_InvokingLevel(3, (user) => {
			//  And: user selects a1
			user.Choose( space.Label );
			//  And: user choses not to replace (this is what we are testing...)
			user.Choose( "Done" );
		});

		// Then:
		fxt.GameState.Tokens[space].InvaderSummary().ShouldBe( "1C@3" );
	}

	static void InitMountainThenAllSands(GameState gs) {
		var sand = InvaderCard.Stage1(Terrain.Sands);
		gs.InitTestInvaderDeck(
			InvaderCard.Stage1(Terrain.Mountain), // initial explorer in mountains
			sand, sand, sand, sand, sand
		);
	}

	static void ClearBlight() {
		// So it doesn't cascade during ravage
		foreach(Space space in ActionScope.Current.Spaces_Unfiltered)
			space.Init( Token.Blight, 0 ); // Don't trigger events
	}

	readonly List<string> _log = [];

}

