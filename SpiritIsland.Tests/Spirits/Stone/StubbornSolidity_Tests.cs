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
		tokens.InitDefault(Human.Dahan, 0);

		//   And: enough explorers to cause blight
		tokens.InitDefault(Human.Explorer, 2);

		//  When: Card Played  (grow,select card,play card)
		//   And: Invader actions proceed
		Stone_Grows( fxt );
		BuysAndUses( fxt, StubbornSolidity.Name );
		fxt.user.WaitForNext();

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
		tokens.InitDefault(Human.Dahan, dahanCount);

		//   And: enough explorers to cause blight
		tokens.InitDefault(Human.Explorer, 2);

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
	public async Task LotsOfInvaders_DahanUnchanged_InvadersDamaged() {
		Spirit spirit = new StonesUnyieldingDefiance();
		Board board = Board.BuildBoardA(); 
		GameState gameState = new GameState(spirit,board);
		gameState.BlightCard.OnGameStart( gameState );
		gameState.IslandWontBlight();
		Space space = gameState.Tokens[ board[5] ];

		// Given: 4 dahan, 10 explorers
		space.InitDefault(Human.Dahan, 4 );
		space.InitDefault(Human.Explorer, 10 );

		//   And: Played StubbornSolidity
		await Play_StubbornSolidity_On( spirit, space );

		//  When: Ravaging
		await space.SpaceSpec.When_Ravaging();

		//  Then: All 4 dahan remain
		//   And: 10-4=6 points of damage to the land => Blight,
		//   And: Dahan deal 8 damage leaving 2 explorers
		space.Summary.ShouldBe("1B,4D@2,2E@1,4G"); // 'G' = defend
	}

	// Stops Power Damaged / Destroyed

	[Trait( "Feature", "Frozen" )]
	[Fact]
	public async Task FrozenDahan_DontMove() {
		Spirit spirit = new StonesUnyieldingDefiance();
		Board board = Board.BuildBoardA();
		GameState gameState = new GameState( spirit, board );
		Space targetSpace = gameState.Tokens[board[8]];
		Space adjacentSpace = gameState.Tokens[board[7]];

		// Given: 3 dahan & presence in Target
		targetSpace.InitDefault( Human.Dahan, 3 );
		spirit.Given_IsOn( targetSpace, 3 );
		//   And: 2 dahan in Adjacent
		adjacentSpace.InitDefault( Human.Dahan, 2 );

		//   And: Played StubbornSolidity on target
		await Play_StubbornSolidity_On( spirit, targetSpace );
		//   And: Played StubbornSolidity on adjacent
		await Play_StubbornSolidity_On( spirit, adjacentSpace );

		// ! We are allowing user to select non-movable pieces because:
			// Easier to check ambient moveable after user has made choice.

		//  When: Playing card that Gathers and Pushes - Call to Migrate
		await spirit.When_ResolvingCard<CallToMigrate>( (user) => {
			user.NextDecision.HasPrompt( CallToMigrate.Name + ": Target Space" ).HasOptions( "A5,A6,A7,A8" ).Choose( targetSpace.SpaceSpec );
			// Gather - 3
			user.NextDecision.HasPrompt( "Gather up to (2)" ).HasSourceOptions( "D@2,Done" ).MoveFrom( "D@2" ); // ! This is showing (2) because there are only 2 there.
			user.NextDecision.HasPrompt( "Gather up to (2)" ).HasSourceOptions( "D@2,Done" ).MoveFrom( "D@2" );
			user.NextDecision.HasPrompt( "Gather up to (1)" ).HasSourceOptions( "D@2,Done" ).MoveFrom( "D@2" );
			// Push
			user.NextDecision.HasPrompt( "Push up to (3)" ).HasSourceOptions( "D@2,Done" ).MoveFrom( "D@2" ).HasDestinationOptions( "A5,A6,A7" ).MoveTo( "A5" );
			user.NextDecision.HasPrompt( "Push up to (2)" ).HasSourceOptions( "D@2,Done" ).MoveFrom( "D@2" ).HasDestinationOptions( "A5,A6,A7" ).MoveTo( "A5" );
			user.NextDecision.HasPrompt( "Push up to (1)" ).HasSourceOptions( "D@2,Done" ).MoveFrom( "D@2" ).HasDestinationOptions( "A5,A6,A7" ).MoveTo( "A5" );
		} );

		//  Then: Dahan are still there (not replaced)
		targetSpace.Summary.ShouldBe( "3D@2,3G,3SUD" );
		adjacentSpace.Summary.ShouldBe( "2D@2,2G" );
		board[5].ScopeSpace.Summary.ShouldBe("[none]"); // nothing goes to 5

	}

	[Trait( "Feature", "Frozen" )]
	[Fact]
	public async Task DahanMovedIn_BecomeFrozen() {
		Spirit spirit = new StonesUnyieldingDefiance();
		Board board = Board.BuildBoardA();
		GameState gameState = new GameState( spirit, board );
		Space targetSpace = gameState.Tokens[board[8]];
		Space adjacentSpace = gameState.Tokens[board[7]];

		// Given: presence in Target
		spirit.Given_IsOn( targetSpace );
		//   And: 3 dahan in Adjacent
		adjacentSpace.InitDefault( Human.Dahan, 2 );

		//   And: Played StubbornSolidity on target
		await Play_StubbornSolidity_On( spirit, targetSpace );

		//  When: Playing card that Gathers and Pushes - Call to Migrate
		await spirit.When_ResolvingCard<CallToMigrate>( (user) => {
			user.NextDecision.HasPrompt( CallToMigrate.Name + ": Target Space" ).HasOptions( "A5,A6,A7,A8" ).Choose( targetSpace.SpaceSpec );
			//   And: outside dahan are gathered
			user.NextDecision.HasPrompt( "Gather up to (2)" ).HasSourceOptions( "D@2,Done" ).MoveFrom( "D@2" );
			user.NextDecision.HasPrompt( "Gather up to (1)" ).HasSourceOptions( "D@2,Done" ).MoveFrom( "D@2" );

			// Push - 3
			user.NextDecision.HasPrompt( "Push up to (2)" )
				.HasSourceOptions( "D@2,Done" ).MoveFrom( "D@2" )
				.HasDestinationOptions( "A5,A6,A7" ).MoveTo( "A5" );

			user.NextDecision.HasPrompt( "Push up to (2)" ).HasSourceOptions( "D@2,Done" ).MoveFrom( "D@2" )
				.HasDestinationOptions( "A5,A6,A7" ).MoveTo( "A5" );

			user.NextDecision.HasPrompt( "Push up to (1)" ).HasSourceOptions( "D@2,Done" ).MoveFrom( "D@2" )
				.HasDestinationOptions( "A5,A6,A7" ).MoveTo( "A5" );

		} );

		//  Then: dahan were gathered
		adjacentSpace.Summary.ShouldBe( "[none]" );

		//   But: Dahan became frozen and were not pushed
		targetSpace.Summary.ShouldBe( "2D@2,1SUD" ); // no defends

		board[5].ScopeSpace.Summary.ShouldBe("[none]");
	}


	[Trait( "Feature", "Frozen" )]
	[Fact]
	public async Task ReplacingDahan_NoChange() {
		Spirit spirit = new StonesUnyieldingDefiance();
		Board board = Board.BuildBoardA();
		GameState gameState = new GameState( spirit, board );
		Space targetSpace = gameState.Tokens[board[5]];

		// Given: dahan & presence
		targetSpace.InitDefault( Human.Dahan, 1 );
		spirit.Given_IsOn( targetSpace );

		//   And: Played StubbornSolidity
		await Play_StubbornSolidity_On( spirit, targetSpace );

		//  When: Playing card that replaces Dahan - Dissolve the Bonds of Kinship
		await spirit.When_ResolvingCard<DissolveTheBondsOfKinship>((user)=> {
			user.NextDecision.HasPrompt( DissolveTheBondsOfKinship.Name + ": Target Space" ).HasOptions( "A1,A4,A5,A6,A7,A8" ).Choose( targetSpace.SpaceSpec );
		} );

		//  Then: Dahan are still there (not replaced)
		targetSpace.Summary.ShouldBe( "1D@2,1G,1SUD" ); // 1G => Defend-1 from Stubborn Solidity
	}

	[Trait( "Feature", "Frozen" )]
	[Fact]
	public async Task TargetingDetectsFrozenDahan() {
		Spirit spirit = new StonesUnyieldingDefiance();
		Board board = Board.BuildBoardA();
		GameState gameState = new GameState( spirit, board );
		Space space = gameState.Tokens[board[5]];

		// Given: dahan & presence
		space.InitDefault( Human.Dahan, 1 );
		spirit.Given_IsOn( space );

		//   And: Played StubbornSolidity
		await Play_StubbornSolidity_On(spirit,space);

		//  When: Playing card that targets a Dahan space CallToTrade
		await using ActionScope uow2 = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power,spirit);
		Task task = PowerCard.For(typeof(CallToTrade)).ActivateAsync(spirit);

		//  Then: can still target space with frozen dahan
		task.IsCompleted.ShouldBeFalse();
		spirit.NextDecision().HasPrompt("Call to Trade: Target Space").HasOptions("A5").Choose("A5");
	}

	static Task Play_StubbornSolidity_On( Spirit spirit, Space targetSpace ) {
		return StubbornSolidity.ActAsync( spirit.Target( targetSpace.SpaceSpec ) ).ShouldComplete(StubbornSolidity.Name);
	}

	static void BuysAndUses( GameFixture fxt, string cardName ) {
		fxt.user.PlaysCard( cardName );
		fxt.user.SelectsFastAction( cardName );
		fxt.user.TargetsLand( cardName, "A1,A2,A3,A4,[A5],A6" );
	}

	static void Stone_Grows( GameFixture fxt ) {
		fxt.user.Growth_SelectAction( "Place Presence(2)" );
		fxt.user.Growth_PlacesPresence( "energy>A1;A2;A3;A4;[A5];A6;A7;A8" );
	}

}

