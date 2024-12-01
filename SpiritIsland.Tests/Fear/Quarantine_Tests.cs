namespace SpiritIsland.Tests.Fear;

public class Quarantine_Tests {

	[Trait( "Invaders", "Explore" )]
	[Trait( "Invaders", "Deck" )]
	[Theory]
	[InlineData(false)] // 1st card is configed as coastal
	[InlineData(true)]  // A1 A2 A3 are coastland and stopped by Fear
	public async Task Level1_ExploreDoesNotAffectCoastland( bool activateFearCard ) {


		// Setup:
		var gs = new GameState(new ShiftingMemoryOfAges(), Boards.A);
		await gs.InvaderDeck.InitExploreSlotAsync();
		var card = gs.InvaderDeck.Explore.Cards.First();
		var matchingSpaces = gs.Spaces_Unfiltered.Where(card.MatchesCard).ToList();
		var coastal = matchingSpaces.Single(x=>x.SpaceSpec.IsCoastal);
		var nonCoastal = matchingSpaces.Single(x => !x.SpaceSpec.IsCoastal);

		// Given: town on both
		coastal.Given_InitSummary("1T@2");
		nonCoastal.Given_InitSummary("1T@2");

		//  When: Level 1 activated - Explore does not affect Coastal lands
		if( activateFearCard )
			await new Quarantine().ActAsync(1);

		//   And: Explore Card Activated
		await gs.InvaderDeck.Explore.Execute(gs);

		//  Then: coastal down is skipped
		coastal.Summary.ShouldBe(activateFearCard?"1T@2":"1E@1,1T@2");
		//   And: other was explored
		nonCoastal.Summary.ShouldBe("1E@1,1T@2");

	}

	[Trait( "Invaders", "Explore" )]
	[Trait( "Invaders", "Deck" )]
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task Level2_ExploreDoesNotAffectCoastlandNorComeFromDiseasedSpots( bool activateFearCard ) {

		// Setup:
		var board = Boards.A;
		var gs = new GameState(new VitalStrength(),board);
		var coastalJungle = board[3].ScopeSpace;
		var inlandJungle = board[8].ScopeSpace;

		// Given: nothing on the coastal Jungle
		coastalJungle.Given_InitSummary("");

		//   And: the only thing around A8 (a jungle) is a diseased town
		board[5].Given_InitSummary("");
		board[6].Given_InitSummary("");
		board[7].Given_InitSummary("1T@2,1Z"); // town & diZease
		inlandJungle.Given_InitSummary("");

		//   And: Played Quarentine-2
		//     - Explore does not affect Coastal lands.
		//     - Lands with Disease are not a source of Invaders when exploring.
		if( activateFearCard )
			await new Quarantine().ActAsync(2);

		//  When: Exploring in the Jungle
		await gs.InvaderDeck.UnrevealedCards.First(x=>x.MatchesCard(inlandJungle))
			.When_Exploring();

		//  Then: no inland explore
		inlandJungle.Summary.ShouldBe(activateFearCard?"[none]":"1E@1");

		//   And: no coastal explore
		coastalJungle.Summary.ShouldBe(activateFearCard ? "[none]" : "1E@1");

	}

	[Trait( "Invaders", "Explore" )]
	[Trait( "Invaders", "Deck" )]
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task Level3_NoCoastalExplore_NoActionInDiseasedLands( bool activateFearCard ) {

		var board = Boards.A;
		var gs = new GameState(new DownpourDrenchesTheWorld(),board);
		var a4Sand = board[4].ScopeSpace;
		var a7Sand = board[7].ScopeSpace;
		var a1Coastal = board[1].ScopeSpace;
		var a2Coastal = board[2].ScopeSpace;
		var a3CoastalJungle = board[3].ScopeSpace;
		var a8Jungle = board[8].ScopeSpace;

		// Given: Ravage lands (sand) have a disease, dahan, explorer
		a4Sand.Given_InitSummary("1D@2,1E@1,1Z");
		a7Sand.Given_InitSummary("1D@2,1E@1,1Z");

		//   And: Build lands (Costal:A1..3) all have explorers, (A1 has a disease too)
		a1Coastal.Given_InitSummary("1E@1");  // Disease added to a1 below
		a2Coastal.Given_InitSummary("1E@1");
		a3CoastalJungle.Given_InitSummary("1E@1");

		// Explore lands (jungle:A3 & A8) have a source and disease
		a1Coastal.Given_HasTokens("1Z"); // A3 is coastal so its source is the ocean
		a8Jungle.Given_InitSummary("1Z"); // 
		board[5].Given_InitSummary("1T@2"); // A8 source

		// And: quarantine-3
		//		if( activateFearCard )
		if(activateFearCard)
			await new Quarantine().ActAsync(3);

		// When: Ravaging in the Sand
		await gs.InvaderDeck.UnrevealedCards.First(x => x.MatchesCard(board[4]))
			.When_Ravaging();

		// Then: Ravage didn't happen in either space  (explorers are still around)
		string expectedRavageResult = activateFearCard ? "1D@2,1E@1,1Z" : "1D@1,1Z";
		a4Sand.Summary.ShouldBe(expectedRavageResult);
		a7Sand.Summary.ShouldBe(expectedRavageResult);

		// When: building on the coast
		await InvaderCard.Stage2Costal().When_Building();

		// Then: 1 Build didn't happened
		a1Coastal.Summary.ShouldBe(activateFearCard?"1E@1,1Z": "1E@1"); // not here, disease!
		a2Coastal.Summary.ShouldBe("1E@1,1T@2");       // this built
		a3CoastalJungle.Summary.ShouldBe("1E@1,1T@2"); // this too!

		// When: exploring Jungle
		await gs.InvaderDeck.UnrevealedCards.First(x => x.MatchesCard(board[8])).When_Exploring();

		// Then: No explore happend
		a3CoastalJungle.Summary.ShouldBe(activateFearCard ? "1E@1,1T@2" : "2E@1,1T@2"); // original
		a8Jungle.Summary.ShouldBe(activateFearCard ? "1Z" : "1E@1,1Z");

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
			board[4].ScopeSpace.SkipRavage("Test");

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

	protected Task<Log.FearCardRevealed> _fearCardRevealed;

	protected VirtualTestUser _user;
	protected Spirit _spirit;
	protected Queue<string> _log = new();

	protected void GrowAndBuyNoCards() {
		_spirit.ClearAllBlight();
		_user.GrowAndBuyNoCards();
	}

	#endregion

}