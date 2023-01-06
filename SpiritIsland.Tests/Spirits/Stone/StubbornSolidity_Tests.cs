namespace SpiritIsland.Tests.Spirits.StoneNS;

public class StubbornSolidity_Tests {

	[Trait("Invaders","Ravage")]
	[Fact]
	public void NoDahan_NoDefend() {
		var fxt = new GameFixture()
			.WithSpirit(new StonesUnyieldingDefiance())
			.Start();

		// Given: a space to ravage on.
		var space = fxt.board[5];
		var tokens = fxt.gameState.Tokens[space];
		fxt.InitRavageCard( space );

		//   And: no dahan
		tokens.InitDefault(TokenType.Dahan, 0);

		//   And: enough explorers to cause blight
		tokens.InitDefault(Invader.Explorer, 2);

		//  When: Card Played  (grow,select card,play card)
		//   And: Invader actions proceed
		Stone_Grows( fxt );
		BuysAndUses( fxt, StubbornSolidity.Name );

		//  Then: Blight
		tokens.Blight.Count.ShouldBe( 1 );
	}

	[Trait("Feature","Frozen")]
	[Trait("Invaders","Ravage")]
	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	public void Dahan_Defends1Each_NoBlight(int dahanCount) {
		var fxt = new GameFixture()
			.WithSpirit(new StonesUnyieldingDefiance())
			.Start();

		// Given: a space to ravage on.
		var space = fxt.board[5]; // a5
		var tokens = fxt.gameState.Tokens[space];
		fxt.InitRavageCard( space );

		//   And: dahan in space
		tokens.InitDefault(TokenType.Dahan, dahanCount);

		//   And: enough explorers to cause blight
		tokens.InitDefault(Invader.Explorer, 2);

		//  When: Card Played  (grow,select card,play card)
		//   And: Invader actions proceed
		Stone_Grows( fxt );
		BuysAndUses( fxt, StubbornSolidity.Name );

		//  Then: No blight
		tokens.Blight.Count.ShouldBe( 0 );
	}

	[Trait( "Feature", "Frozen" )]
	[Trait("Invaders","Ravage")]
	[Fact]
	public void LotsOfInvaders_DahanUnchanged_InvadersDamaged() {
		Spirit spirit = new StonesUnyieldingDefiance();
		Board board = Board.BuildBoardA(); 
		GameState gameState = new GameState(spirit,board);
		SpaceState tokens = gameState.Tokens[ board[5] ];

		// Given: 4 dahan, 10 explorers
		tokens.InitDefault(TokenType.Dahan, 4 );
		tokens.InitDefault(Invader.Explorer, 10 );

		//   And: Played StubbornSolidity
		Play_StubbornSolidity_On( spirit, gameState, tokens );

		//  When: Ravaging
		tokens.DoARavage().Wait();

		//  Then: All 4 dahan remain
		//   And: 10-4=6 points of damage to the land => Blight,
		//   And: Dahan deal 8 damage leaving 2 explorers
		tokens.Summary.ShouldBe("1B,4D@2,2E@1,4G"); // 'G' = defend
	}

	// Stops Power Damaged / Destroyed

	[Trait( "Feature", "Frozen" )]
	[Fact]
	public void FrozenDahan_DontMove() {
		Spirit spirit = new StonesUnyieldingDefiance();
		Board board = Board.BuildBoardA();
		GameState gameState = new GameState( spirit, board );
		SpaceState targetSpace = gameState.Tokens[board[8]];
		SpaceState adjacentSpace = gameState.Tokens[board[7]];

		// Given: 3 dahan & presence in Target
		targetSpace.InitDefault( TokenType.Dahan, 3 );
		spirit.Presence.Adjust( targetSpace, 3 );
		//   And: 3 dahan in Adjacent
		adjacentSpace.InitDefault( TokenType.Dahan, 2 );

		//   And: Played StubbornSolidity on target
		Play_StubbornSolidity_On( spirit, gameState, targetSpace );
		//   And: Played StubbornSolidity on adjacent
		Play_StubbornSolidity_On( spirit, gameState, adjacentSpace );

		//  When: Playing card that Gathers and Pushes - Call to Migrate
		using UnitOfWork uow2 = gameState.StartAction( ActionCategory.Spirit_Power );
		Task task = PowerCard.For<CallToMigrate>().ActivateAsync( spirit.BindMyPowers( gameState, uow2 ) );
		spirit.NextDecision().HasPrompt( CallToMigrate.Name + ": Target Space" ).HasOptions( "A5,A6,A7,A8" ).Choose( targetSpace.Space );

		//  Then: Dahan are still there (not replaced)
		targetSpace.Summary.ShouldBe( "3D@2,3G" );
		adjacentSpace.Summary.ShouldBe( "2D@2,2G" );

		//   And: all done, nothing to push/pull
		task.IsCompleted.ShouldBeTrue();
	}

	[Trait( "Feature", "Frozen" )]
	[Fact]
	public void DahanMovedIn_BecomeFrozen() {
		Spirit spirit = new StonesUnyieldingDefiance();
		Board board = Board.BuildBoardA();
		GameState gameState = new GameState( spirit, board );
		SpaceState targetSpace = gameState.Tokens[board[8]];
		SpaceState adjacentSpace = gameState.Tokens[board[7]];

		// Given: presence in Target
		spirit.Presence.Adjust( targetSpace, 1 );
		//   And: 3 dahan in Adjacent
		adjacentSpace.InitDefault( TokenType.Dahan, 2 );

		//   And: Played StubbornSolidity on target
		Play_StubbornSolidity_On( spirit, gameState, targetSpace );

		//  When: Playing card that Gathers and Pushes - Call to Migrate
		using UnitOfWork uow2 = gameState.StartAction( ActionCategory.Spirit_Power );
		Task task = PowerCard.For<CallToMigrate>().ActivateAsync( spirit.BindMyPowers( gameState, uow2 ) );
		spirit.NextDecision().HasPrompt( CallToMigrate.Name + ": Target Space" ).HasOptions( "A5,A6,A7,A8" ).Choose( targetSpace.Space );

		//   And: outside dahan are gathered
		spirit.NextDecision().HasPrompt( "Gather up to 3 Dahan" ).HasOptions( "D@2 on A7,Done" ).Choose( "D@2 on A7" );
		spirit.NextDecision().HasPrompt( "Gather up to 2 Dahan" ).HasOptions( "D@2 on A7,Done" ).Choose( "D@2 on A7" );

		//  Then: dahan were gathered
		adjacentSpace.Summary.ShouldBe( "" );

		//   But: Dahan became frozen and were not pushed
		targetSpace.Summary.ShouldBe( "2D@2" ); // no defends

		task.IsCompleted.ShouldBeTrue();
	}


	[Trait( "Feature", "Frozen" )]
	[Fact]
	public void ReplacingDahan_NoChange() {
		Spirit spirit = new StonesUnyieldingDefiance();
		Board board = Board.BuildBoardA();
		GameState gameState = new GameState( spirit, board );
		SpaceState targetSpace = gameState.Tokens[board[5]];

		// Given: dahan & presence
		targetSpace.InitDefault( TokenType.Dahan, 1 );
		spirit.Presence.Adjust( targetSpace, 1 );

		//   And: Played StubbornSolidity
		Play_StubbornSolidity_On( spirit, gameState, targetSpace );

		//  When: Playing card that replaces Dahan - Dissolve the Bonds of Kinship
		using UnitOfWork uow2 = gameState.StartAction( ActionCategory.Spirit_Power );
		Task task = PowerCard.For<DissolveTheBondsOfKinship>().ActivateAsync( spirit.BindMyPowers( gameState, uow2 ) );
		//   And: target space with frozen dahan
		task.IsCompleted.ShouldBeFalse();
		spirit.NextDecision().HasPrompt( DissolveTheBondsOfKinship.Name + ": Target Space" ).HasOptions( "A1,A4,A5,A6,A7,A8" ).Choose( targetSpace.Space );

		//  Then: Dahan are still there (not replaced)
		targetSpace.Summary.ShouldBe( "1D@2,1G" ); // 1G => Defend-1 from Stubborn Solidity
		//   And: all done
		task.IsCompleted.ShouldBeTrue();
	}

	[Trait( "Feature", "Frozen" )]
	[Fact]
	public void TargetingDetectsFrozenDahan() {
		Spirit spirit = new StonesUnyieldingDefiance();
		Board board = Board.BuildBoardA();
		GameState gameState = new GameState( spirit, board );
		SpaceState spaceState = gameState.Tokens[board[5]];

		// Given: dahan & presence
		spaceState.InitDefault( TokenType.Dahan, 1 );
		spirit.Presence.Adjust(spaceState,1);

		//   And: Played StubbornSolidity
		Play_StubbornSolidity_On(spirit,gameState,spaceState);

		//  When: Playing card that targets a Dahan space CallToTrade
		using UnitOfWork uow2 = gameState.StartAction( ActionCategory.Spirit_Power );
		Task task = PowerCard.For<CallToTrade>().ActivateAsync(spirit.BindMyPowers(gameState,uow2));

		//  Then: can still target space with frozen dahan
		task.IsCompleted.ShouldBeFalse();
		spirit.NextDecision().HasPrompt("Call to Trade: Target Space").HasOptions("A5").Choose("A5");
	}

	static void Play_StubbornSolidity_On( Spirit spirit, GameState gameState, SpaceState targetSpace ) {
		using UnitOfWork actionScope = gameState.StartAction( ActionCategory.Spirit_Power );
		StubbornSolidity.ActAsync( spirit.BindMyPowers( gameState, actionScope ).Target( targetSpace.Space ) ).Wait();
	}

	static void BuysAndUses( GameFixture fxt, string cardName ) {
		fxt.user.PlaysCard( cardName );
		fxt.user.SelectsFastAction( cardName );
		fxt.user.TargetsLand( cardName, "A1,A2,A3,A4,[A5],A6" );
	}

	static void Stone_Grows( GameFixture fxt ) {
		fxt.user.Growth_SelectAction( "PlacePresence(2)" );
		fxt.user.Growth_PlacesPresence( "energy>A1;A2;A3;A4;[A5];A6;A7;A8" );
	}

}

