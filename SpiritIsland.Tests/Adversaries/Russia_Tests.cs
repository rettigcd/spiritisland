namespace SpiritIsland.Tests.Adversaries;

public class Russia_Tests {

	[Fact]
	public void Level0_Escalation_AddExplorersToBeastSpaces() {

		// Given: Russia, Level 0
		var cfg = Given_RussiaLevel(0).SetBoards("A").SetSpirits(RiverSurges.Name);
		GameState gameState = BuildGame(cfg);
		Board board = gameState.Island.Boards[0];

		//  And: no explorers on 2 spaces
		SpaceState beastSpace1 = gameState.Tokens[ board[1] ];
		SpaceState beastSpace2 = gameState.Tokens[ board[4] ];
		beastSpace1.InitDefault(Invader.Explorer,0);
		beastSpace2.InitDefault( Invader.Explorer, 0 );

		//   And: a stage-2 escalation card
		var a8 = gameState.Tokens[board[8]];
		IInvaderCard card = gameState.InvaderDeck.UnrevealedCards.First(x=>x.InvaderStage==2 && x.MatchesCard( a8 ) );

		// Given: beasts on 2 other spaces
		beastSpace1.Beasts.Adjust(1);
		beastSpace2.Beasts.Adjust(1);

		// When: exploring and escalation card
		Task t = card.Explore(gameState);

		// Then: user can place 2 explorers in either of the 2 beast spaces
		Spirit spirit = gameState.Spirits[0];
		t.IsCompleted.ShouldBeFalse();
		spirit.NextDecision().HasPrompt( "Escalation - Add Explorer for board A (1 of 2)" ).HasOptions("A1,A4").Choose(beastSpace1.Space.Text);
		t.IsCompleted.ShouldBeFalse();
		spirit.NextDecision().HasPrompt( "Escalation - Add Explorer for board A (2 of 2)" ).HasOptions( "A1,A4" ).Choose( beastSpace2.Space.Text );

		beastSpace1.Sum(Invader.Explorer).ShouldBe(1);
		beastSpace2.Sum( Invader.Explorer ).ShouldBe( 1 );

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
		beastSpace1.InitDefault( Invader.Explorer, 0 );
		beastSpace2.InitDefault( Invader.Explorer, 0 );
		beastSpace1.Beasts.Adjust( 1 );
		beastSpace2.Beasts.Adjust( 1 );

		//   And: a stage-2 escalation card
		var a8 = gameState.Tokens[board1[8]];
		IInvaderCard card = gameState.InvaderDeck.UnrevealedCards.First( x => x.InvaderStage == 2 && x.MatchesCard( a8 ));

		// When: exploring and escalation card
		Task t = card.Explore( gameState );

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

		beastSpace1.Sum( Invader.Explorer ).ShouldBe( 2 );
		beastSpace2.Sum( Invader.Explorer ).ShouldBe( 3 ); // +1 normal explore

		// All done
		t.IsCompleted.ShouldBeTrue();
	}

	[Fact]
	public void Level1_Placement_And2Damage() {
		// Given: Russia, Level 1
		var cfg = Given_RussiaLevel( 1 ).SetBoards( "A" ).SetSpirits( RiverSurges.Name );
		GameState gameState = BuildGame( cfg );
		Board board = gameState.Island.Boards[0];

		//  Then: A7 should have get 1 beast and 1 explorer
		SpaceState a7 = gameState.Tokens[board[7]];
		a7.Summary.ShouldBe( "1A,2D@2,1E@1" ); // 1A is beast

		//  When: Ravage on that space
		a7.DoARavage().Wait();

		//  Then: explorer did 2 damage and killed 1 dahan and created 1 blight and removed the Beast
		a7.Summary.ShouldBe( "1B,1D@2" );
	}

	[Fact]
	public void Level2_Ravage_SaveExplorer() {
		var cfg = Given_RussiaLevel( 2 ).SetBoards("A").SetSpirits(RiverSurges.Name);
		GameState gameState = BuildGame( cfg );
		Board boardA = gameState.Island.Boards[0];
		SpaceState a5 = gameState.Tokens[boardA[5]];
		// Given: 2 explorers and 10 dahan on a space
		a5.InitTokens("10D@2,2E@1");
		//   And: no explorers on push-destination
		SpaceState a1 = gameState.Tokens[boardA[1]];
		a1.Clear();
		//  When: ravage
		Task t = a5.DoARavage();
		//  Then: we push 1
		t.IsCompleted.ShouldBeFalse();
		Spirit spirit = gameState.Spirits[0];
		spirit.NextDecision().HasPrompt( "Push E@1 to" ).HasOptions( "A1,A4,A6,A7,A8" ).Choose(a1.Space.Text);
		t.IsCompleted.ShouldBeTrue();

		//   And: explorer should be on destination
		a1.Summary.ShouldBe("1E@1");

		//   And: remaining explorers is 0
		a5.Summary.ShouldBe("1B,8D@2");
	}

	[Fact]
	public void Level2_PowerOnCoast_PushExplorerToLandOnly() {
		var cfg = Given_RussiaLevel( 2 ).SetBoards( "A" ).SetSpirits( RiverSurges.Name );
		GameState gameState = BuildGame( cfg );
		Spirit spirit = gameState.Spirits[0];
		Board boardA = gameState.Island.Boards[0];
		SpaceState a3 = gameState.Tokens[boardA[3]];
		// Given: 10 explorers on a space
		a3.InitTokens( "10E@1" );
		//   And: no explorers on push-destination
		SpaceState destination = gameState.Tokens[boardA[4]];
		destination.Clear();
		//  When: power destroys
		using var unitOfWork = gameState.StartAction( ActionCategory.Spirit_Power );
		Task t = TheJungleHungers.ActAsync( spirit.BindMyPowers( gameState, unitOfWork ).Target(a3.Space) );
		//  Then: we push 1 explorer to a Land-space (not A0-ocean)
		t.IsCompleted.ShouldBeFalse();
		spirit.NextDecision().HasPrompt( "Push E@1 to" ).HasOptions( "A2,A4" ).Choose( destination.Space.Text );
		t.IsCompleted.ShouldBeTrue();

		//   And: explorer should be on destination
		destination.Summary.ShouldBe( "1E@1" );

		//   And: remaining explorers is 0
		a3.Summary.ShouldBe( "" );
	}

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
		a5.Clear().InitTokens("1D@2,3E@1");

		//  When: card ravages
		Task t = card.Ravage(gameState);

		//  Then: dahan is destroyed and land is blighted
		a5.Summary.ShouldBe("1B,3E@1");

		t.IsCompleted.ShouldBeTrue();
	}

	[Fact]
	public void Level3_3InvadersDontBuildNorExplore() {
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
		a5.Clear().InitTokens( orig );

		//  When: card builds & explore
		card.Build( gameState ).Wait();
		card.Explore( gameState ).Wait();

		//  Then: no change
		a5.Summary.ShouldBe( orig );
	}


	// !!! Level 2 - Test/Implement - Isolate prevents pushing explorer
	// !!! Level 5 - Test
	// !!! Level 6 - Test

	static GameConfiguration Given_RussiaLevel( int level ) => new GameConfiguration { Adversary = new AdversaryConfig( Russia.Name, level ), ShuffleNumber = 1, };
	static GameState BuildGame( GameConfiguration cfg ) => ConfigurableTestFixture.GameBuilder.BuildGame( cfg );
}
