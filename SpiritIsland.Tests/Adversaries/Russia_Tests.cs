namespace SpiritIsland.Tests.Adversaries;

public class Russia_Tests {

	#region Level 0

	[Fact]
	public void Level0_Escalation_AddExplorersToBeastSpaces() {

		// Given: Russia, Level 0
		var cfg = Given_RussiaLevel(0).SetBoards("A").SetSpirits(RiverSurges.Name);
		GameState gameState = BuildGame(cfg);
		Board board = gameState.Island.Boards[0];

		//  And: no explorers on 2 spaces
		SpaceState beastSpace1 = gameState.Tokens[ board[1] ];
		SpaceState beastSpace2 = gameState.Tokens[ board[4] ];
		beastSpace1.InitDefault(Human.Explorer,0);
		beastSpace2.InitDefault( Human.Explorer, 0 );

		//   And: a stage-2 escalation card
		var a8 = gameState.Tokens[board[8]];
		InvaderCard card = gameState.InvaderDeck.UnrevealedCards.First(x=>x.InvaderStage==2 && x.MatchesCard( a8 ) );

		// Given: beasts on 2 other spaces
		gameState.Tokens[ board[5] ].Beasts.Init(0);
		beastSpace1.Beasts.Adjust(1);
		beastSpace2.Beasts.Adjust(1);

		// When: exploring and escalation card
		Task t = gameState.InvaderDeck.Explore.Engine.ActivateCard( card, gameState ); 

		// Then: user can place 2 explorers in either of the 2 beast spaces
		Spirit spirit = gameState.Spirits[0];
		t.IsCompleted.ShouldBeFalse();
		spirit.NextDecision().HasPrompt( "Escalation - Add Explorer for board A (1 of 2)" )
			.HasOptions("A1,A4")
			.Choose(beastSpace1.Space.Text);
		t.IsCompleted.ShouldBeFalse();
		spirit.NextDecision().HasPrompt( "Escalation - Add Explorer for board A (2 of 2)" )
			.HasOptions( "A1,A4" )
			.Choose( beastSpace2.Space.Text );

		beastSpace1.Sum(Human.Explorer).ShouldBe(1);
		beastSpace2.Sum( Human.Explorer ).ShouldBe( 1 );

		// All done
		t.IsCompleted.ShouldBeTrue();
	}

	[Fact]
	public void Level0_Escalation_AddExplorersToOtherBoardBeastSpaces() {

		// Given: Russia, Level 0 & 2 spirits
		var cfg = Given_RussiaLevel(0).SetBoards( "A", "B" ).SetSpirits( RiverSurges.Name, LureOfTheDeepWilderness.Name );
		GameState gameState = BuildGame( cfg );
		Board board0 = gameState.Island.Boards[0];
		Board board1 = gameState.Island.Boards[1];

		//  And: no beasts on the first board
		foreach(Space space in board0.Spaces )
			gameState.Tokens[space].Beasts.Init(0);

		//  And: board 2 has 2 spaces with beasts and no explorers
		SpaceState beastSpace1 = gameState.Tokens[board1[5]];
		SpaceState beastSpace2 = gameState.Tokens[board1[8]];
		beastSpace1.InitDefault( Human.Explorer, 0 );
		beastSpace2.InitDefault( Human.Explorer, 0 );
		beastSpace1.Beasts.Adjust( 1 );
		beastSpace2.Beasts.Adjust( 1 );

		//   And: a stage-2 escalation card
		var a8 = gameState.Tokens[board1[8]];
		InvaderCard card = gameState.InvaderDeck.UnrevealedCards.First( x => x.InvaderStage == 2 && x.MatchesCard( a8 ));

		// When: exploring and escalation card
		Task t = gameState.InvaderDeck.Explore.Engine.ActivateCard( card, gameState );

		// Then: user can place 2 explorers in either of the 2 beast spaces
		Spirit spirit0 = gameState.Spirits[0];
		t.IsCompleted.ShouldBeFalse();
		spirit0.NextDecision().HasPrompt( "Escalation - Add Explorer for board A (1 of 2)" ).HasOptions( "B5,B8" ).Choose( beastSpace1.Space.Text );
		t.IsCompleted.ShouldBeFalse();
		spirit0.NextDecision().HasPrompt( "Escalation - Add Explorer for board A (2 of 2)" ).HasOptions( "B5,B8" ).Choose( beastSpace2.Space.Text );

		Spirit spirit1 = gameState.Spirits[1];
		t.IsCompleted.ShouldBeFalse();
		spirit1.NextDecision().HasPrompt( "Escalation - Add Explorer for board B (1 of 2)" ).HasOptions( "B5,B8" ).Choose( beastSpace1.Space.Text );
		t.IsCompleted.ShouldBeFalse();
		spirit1.NextDecision().HasPrompt( "Escalation - Add Explorer for board B (2 of 2)" ).HasOptions( "B5,B8" ).Choose( beastSpace2.Space.Text );

		beastSpace1.Sum( Human.Explorer ).ShouldBe( 2 );
		beastSpace2.Sum( Human.Explorer ).ShouldBe( 3 ); // +1 normal explore

		// All done
		t.IsCompleted.ShouldBeTrue();
	}

	#endregion Level 0

	#region Level 1

	[Fact]
	public async Task Level1_Placement_And2Damage() {
		// Given: Russia, Level 1
		var cfg = Given_RussiaLevel( 1 ).SetBoards( "A" ).SetSpirits( RiverSurges.Name );
		GameState gameState = BuildGame( cfg );
		Board board = gameState.Island.Boards[0];

		//  Then: A7 should have get 1 beast and 1 explorer
		SpaceState a7 = gameState.Tokens[board[7]];
		a7.Summary.ShouldBe( "1A,2D@2,1E@1" ); // 1A is beast

		//  When: Ravage on that space
		await a7.Space.When_Ravaging();

		//  Then: explorer did 2 damage and killed 1 dahan and created 1 blight and removed the Beast
		a7.Summary.ShouldBe( "1B,1D@2" );
	}

	#endregion Level 1

	#region Level 2

	[Fact]
	public async Task Level2_Ravage_SaveExplorer() {
		var cfg = Given_RussiaLevel( 2 ).SetBoards("A").SetSpirits(RiverSurges.Name);
		GameState gameState = BuildGame( cfg );
		Board boardA = gameState.Island.Boards[0];
		SpaceState a5 = gameState.Tokens[boardA[5]];
		// Given: 2 explorers and 10 dahan on a space
		a5.Given_HasTokens("10D@2,2E@1");
var xxx = a5.Keys.ToArray();
		//   And: no explorers on push-destination
		SpaceState a1 = gameState.Tokens[boardA[1]];
		a1.Clear();

		//  When: ravage
		//  Then: we push 1
		Spirit spirit = gameState.Spirits[0];
		await a5.Space.When_Ravaging()
			.AwaitUser( spirit, u => u.NextDecision.HasPrompt( "Push E@1 to" ).HasOptions( "A1,A4,A6,A7,A8" ).Choose( a1.Space.Text ) )
			.ShouldComplete("Ravage");

		//   And: explorer should be on destination
		a1.Summary.ShouldBe("1E@1");

		//   And: remaining explorers is 0
		a5.Summary.ShouldBe("1B,8D@2");
	}

	[Fact]
	public async Task Level2_PowerOnCoast_PushExplorerToLandOnly() {
		var cfg = Given_RussiaLevel( 2 ).SetBoards( "A" ).SetSpirits( RiverSurges.Name );
		GameState gameState = BuildGame( cfg );
		Spirit spirit = gameState.Spirits[0];
		Board boardA = gameState.Island.Boards[0];
		SpaceState a3 = gameState.Tokens[boardA[3]];
		// Given: 10 explorers on a space
		a3.Given_HasTokens( "10E@1" );
		//   And: no explorers on push-destination
		SpaceState destination = gameState.Tokens[boardA[4]];
		destination.Clear();
		//  When: power destroys
		await using var actionScope = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power,spirit);
		Task t = TheJungleHungers.ActAsync( spirit.Target(a3.Space) );
		//  Then: we push 1 explorer to a Land-space (not A0-ocean)
		t.IsCompleted.ShouldBeFalse();
		spirit.NextDecision().HasPrompt( "Push E@1 to" ).HasOptions( "A2,A4" ).Choose( destination.Space.Text );
		t.IsCompleted.ShouldBeTrue();

		//   And: explorer should be on destination
		destination.Summary.ShouldBe( "1E@1" );

		//   And: remaining explorers is 0
		a3.Summary.ShouldBe( "[none]" );
	}

	[Fact]
	public async Task Level2_DestroyingAllExplorers_Pushes1() {
		var cfg = Given_RussiaLevel( 2 ).SetBoards( "A" ).SetSpirits( RiverSurges.Name );
		GameState gameState = BuildGame( cfg );
		Board boardA = gameState.Island.Boards[0];
		Space a5 = boardA[5];

		// Given: many explorers on A5
		a5.Tokens.Given_HasTokens( "5E@1" );
		//   And: nothing on A6
		Space a6 = boardA[6];
		a6.Given_ClearTokens();

		// When destroying all explorers
		await gameState.Spirits[0].When_TargetingSpace( a5, TheJungleHungers.ActAsync, ( user ) => {
			// Then: pushes 1
			user.NextDecision.HasPrompt( "Push E@1 to" ).HasOptions( "A1,A4,A6,A7,A8" ).Choose( "A6" );
		} );
		boardA[6].Tokens.Summary.ShouldBe( "1E@1" );
	}

	[Fact]
	public async Task Level2_IsolateEliminatesDestinatoin() {
		var cfg = Given_RussiaLevel( 2 ).SetBoards( "A" ).SetSpirits( RiverSurges.Name );
		GameState gameState = BuildGame( cfg );
		Board boardA = gameState.Island.Boards[0];
		Space a5 = boardA[5];

		// Given: many explorers on A5
		a5.Tokens.Given_HasTokens( "5E@1" );
		//   And: nothing on A6
		Space a6 = boardA[6];
		a6.Given_ClearTokens();
		//   And: A7 is isolated
		boardA[7].Tokens.Init( Token.Isolate, 1 );

		// When destroying all explorers
		await gameState.Spirits[0].When_TargetingSpace( a5, TheJungleHungers.ActAsync, ( user ) => {
			// Then: A7 is missing from push options
			user.NextDecision.HasPrompt( "Push E@1 to" ).HasOptions( "A1,A4,A6,A8" ).Choose( "A6" );
		} );
	}

	[Fact]
	public async Task Level2_IsolateTargetPreventsPush() {
		var cfg = Given_RussiaLevel( 2 ).SetBoards( "A" ).SetSpirits( RiverSurges.Name );
		GameState gameState = BuildGame( cfg );
		Board boardA = gameState.Island.Boards[0];
		Space a5 = boardA[5];

		// Given: many explorers on A5
		a5.Tokens.Given_HasTokens( "5E@1" );
		//   And: is isolated
		a5.Tokens.Init( Token.Isolate, 1 );

		// When destroying all explorers
		await gameState.Spirits[0].When_TargetingSpace( a5, TheJungleHungers.ActAsync, ( user ) => {
			// Then: completes without pushing
		} );
	}

	#endregion Level 2

	#region Level 3

	[Fact]
	public void Level3_3InvadersRavage() {
		var cfg = Given_RussiaLevel( 3 ).SetBoards( "A" ).SetSpirits( RiverSurges.Name );
		GameState gameState = BuildGame( cfg );
		Board boardA = gameState.Island.Boards[0];

		// Given: an invader card from the current GameState
		var card = gameState.InvaderDeck.Explore.Cards.First();
		card.Text.ShouldBe("J"); // jungle

		//   And: a space that does not match the card
		SpaceState a5 = gameState.Tokens[boardA[5]];
		card.MatchesCard( a5 ).ShouldBeFalse();

		//   And: 3 explorers and 1 dahan on space
		a5.Clear().Given_HasTokens("1D@2,3E@1");

		//  When: card ravages (Can't do card.When_Ravaging because we are testing Russias replacement of the Engine)
		gameState.InvaderDeck.Ravage.Engine.ActivateCard( card, gameState ).Wait();

		//  Then: dahan is destroyed and land is blighted
		a5.Summary.ShouldBe("1B,3E@1");

	}

	[Fact]
	public async Task Level3_3InvadersDontBuildNorExplore() {
		var cfg = Given_RussiaLevel( 3 ).SetBoards( "A" ).SetSpirits( RiverSurges.Name );
		GameState gameState = BuildGame( cfg );
		Board boardA = gameState.Island.Boards[0];

		// Given: an invader card from the current GameState
		var card = gameState.InvaderDeck.Explore.Cards.First();
		card.Text.ShouldBe( "J" ); // jungle

		//   And: a space that does not match the card
		SpaceState a5 = gameState.Tokens[boardA[5]];
		card.MatchesCard( a5 ).ShouldBeFalse();

		//   And: 3 explorers and 1 dahan on space
		const string orig = "1D@2,3E@1";
		a5.Clear().Given_HasTokens( orig );

		//  When: card builds & explore
		await card.When_Building();
		await card.When_Exploring();

		//  Then: no change
		a5.Summary.ShouldBe( orig );
	}

	#endregion Level 3

	[Fact]
	public void Level5_InvaderCardsInFearDeck() {
		// Given: russia-5
		GameConfiguration cfg = Given_RussiaLevel( 5 ).SetBoards( "A" ).SetSpirits( RiverSurges.Name );
		GameState gameState = BuildGame( cfg );
		List<InvaderCard> buildCards = gameState.InvaderDeck.Build.Cards;
		int startingCount = buildCards.Count;
		void ActivateCard() => gameState.Fear.AddDirect( new FearArgs( 4 ) );
		void HasAdditionalBuilds( int additionalBuilds ) => buildCards.Count.ShouldBe( startingCount + additionalBuilds );
		//  And: Activating 1..2 cards
		ActivateCard(); // 1
		ActivateCard(); // 2

		// Activating 3 increments build count 0 => 1
		HasAdditionalBuilds( 0 );
		ActivateCard(); // 3
		HasAdditionalBuilds( 1 );

		ActivateCard(); // 4
		ActivateCard(); // 5
		ActivateCard(); // 6

		// Activating 7 increments build count 1 => 2
		HasAdditionalBuilds( 1 );
		ActivateCard(); // 7
		HasAdditionalBuilds( 2 );
	}

	// Level 6 - Not testing, I will never play it.

	static GameConfiguration Given_RussiaLevel( int level ) => new GameConfiguration { Adversary = new AdversaryConfig( Russia.Name, level ), ShuffleNumber = 1, };
	static GameState BuildGame( GameConfiguration cfg ) => ConfigurableTestFixture.GameBuilder.BuildGame( cfg );
}
