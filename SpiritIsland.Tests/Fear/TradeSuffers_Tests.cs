namespace SpiritIsland.Tests.Fear;

public class TradeSuffers_Tests {

	void Init() {
		// On Board-A,
		// use A7 (Sands-2 Dahan)
		// or A4 (Sands-no dahan)
		var (user, spirit) = TestSpirit.StartGame( PowerCard.For(typeof(RiversBounty)), gs => {
			var fear = gs.Fear;
			AvoidTheDahan_Tests.InitMountainThenAllSands( gs );
			gs.NewLogEntry += ( s ) => _log.Add( s.Msg() );
		} );
		_user = user;
		_spirit = spirit;
	}

	[Fact]
	[Trait( "Invaders", "Build" )]
	public void Level1_CityIsNotDamagedDuringRavage_NoBuild() {
		Init();

		// Disable destroying presence
		GameState.Current.DisableBlightEffect();

		// Invaders do not Build in lands with City.

		// Fill all Invaders spaces with the A7 card
		ClearBlight_GrowAndBuyNoCards(); // All of Round 1 - stops at round 2
		ClearBlight_GrowAndBuyNoCards(); // start of round 2
		_user.WaitForNext();			 // start of round 3

		ActivateFearCard( new TradeSuffers() );

		// Given: 1 city and nothing else
		var spaceCtx = _spirit.TargetSpace( "A7" );
		spaceCtx.Tokens.Init("1C@3");

		// When: activating fear
		ClearBlight_GrowAndBuyNoCards();
		_user.AcknowledgesFearCard( FearCard );
		_user.WaitForNext(); // start of round 4

		// Ravage: no dahan, no change:			1B@1,1C@3
		// Build: City present => no build		1B@1,1C@3
		// Explore: +1							1B@1,1C@3,1E@1
		spaceCtx.Tokens.Summary.ShouldBe( "1B,1C@3,1E@1" );
	}

	private const string FearCard = "Trade Suffers : 1 : Invaders do not Build in lands with City.";

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
		AvoidTheDahan_Tests.InitMountainThenAllSands( gs );
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
		ActivateFearCard( new TradeSuffers() );

		// Given: 1 city and a enough dahan to kill the city but not the last explorer
		SpaceState tokens = board[7].Tokens; // _ctx.TargetSpace( "A7" ).Tokens;
		tokens.Init( "1C@3,4D@2,2E@1" );

		// When: activating Fear & Doing Invader Actions
		Task t = InvaderPhase.ActAsync(gs);
		user.AcknowledgesFearCard( FearCard );
		await t.ShouldComplete();
		await gs.TriggerTimePasses(); // let them heal

		// Ravage-a: 1 city + 2 explorers do 5 damage killing 2 dahan    1B@1,1C@1,2D@2,2E@1
		// Ravage-b: 2 dahan do 4 damage killing city and 1 explorer     1B@1,2D@2,1E@1 
		// Build: no city present => build		1B@1,2D@2,1E@1,1T@2
		// Explore: +1							1B@1,2D@2,2E@1,1T@2
		tokens.Summary.ShouldBe( "1B,2D@2,2E@1,1T@2" );

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
		await new TradeSuffers().When_InvokingLevel(3, () => {
			//  And: user selects a1
			fxt.Choose( space.Text );
			//  And: user choses not to replace (this is what we are testing...)
			fxt.Choose( "Done" );
		});

		// Then:
		fxt.GameState.Tokens[space].InvaderSummary().ShouldBe( "1C@3" );
	}

	void ClearBlight_GrowAndBuyNoCards() {
		ClearBlight();
		_user.GrowAndBuyNoCards();
	}

	static void ClearBlight() {
		// So it doesn't cascade during ravage
		foreach(SpaceState space in GameState.Current.Spaces_Unfiltered)
			space.Init( Token.Blight, 0 ); // Don't trigger events
	}

	static void ActivateFearCard(IFearCard fearCard) {
		var fear = GameState.Current.Fear;
		fear.Deck.Pop(); // remove old
		fear.PushOntoDeck( fearCard );
		fear.Add( fear.PoolMax );
	}

	VirtualTestUser _user;
	Spirit _spirit;
	readonly List<string> _log = new();

}

