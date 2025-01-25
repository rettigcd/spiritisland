namespace SpiritIsland.Tests.Spirits.StoneNS;

public class StubbornSolidity_Tests {

	[Trait("Invaders","Ravage")]
	[Fact]
	public async Task NoDahan_NoDefend() {

		var gs = new SoloGameState();

		// Given: a space to ravage on.
		var space = gs.Board[6].ScopeSpace;
		//   And: Dahan on space
		space.InitDefault(Human.Dahan, 0);
		//   And: enough explorers to cause blight
		space.InitDefault(Human.Explorer, 2);
		//   And: Stuborn Solidity played on space
		await StubbornSolidity.ActAsync(gs.Spirit.Target(space));

		//  When: ravaing on that space
		await space.Ravage();

		//  Then: blight = 1
		space.Blight.Count.ShouldBe(1);
	}

	[Trait("Feature","Frozen")]
	[Trait("Invaders","Ravage")]
	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	public async Task Dahan_Defends1Each_NoBlight(int dahanCount) {

		var gs = new SoloGameState();

		// Given: a space to ravage on.
		var space = gs.Board[6].ScopeSpace;
		//   And: Dahan on space
		space.InitDefault(Human.Dahan, dahanCount);
		//   And: enough explorers to cause blight
		space.InitDefault(Human.Explorer, 2);
		//   And: Stuborn Solidity played on space
		await StubbornSolidity.ActAsync(gs.Spirit.Target(space));

		//  When: ravaing on that space
		await space.Ravage();

		//  Then: No blight
		space.Blight.Count.ShouldBe( 0 );
	}

	[Trait( "Feature", "Frozen" )]
	[Trait("Invaders","Ravage")]
	[Fact]
	public async Task LotsOfInvaders_DahanUnchanged_InvadersDamaged() {
		Spirit spirit = new StonesUnyieldingDefiance();
		Board board = Boards.A; 
		GameState gameState = new SoloGameState(spirit,board);
		gameState.BlightCard.OnGameStart( gameState );
		gameState.IslandWontBlight();
		Space space = gameState.Tokens[ board[5] ];

		// Given: 4 dahan, 10 explorers
		space.InitDefault(Human.Dahan, 4 );
		space.InitDefault(Human.Explorer, 10 );

		//   And: Played StubbornSolidity
		await Play_StubbornSolidity_On( spirit, space );

		//  When: Ravaging
		await space.SpaceSpec.When_CardRavages();

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
		Board board = Boards.A;
		GameState gameState = new SoloGameState( spirit, board );
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
			user.NextDecision.HasPrompt( CallToMigrate.Name + ": Target Space" ).HasOptions( "A5,A6,A7,A8" ).Choose( targetSpace );
			// Gather - 3
			user.NextDecision.HasPrompt( "Gather up to (2)" ).HasFromOptions( "D@2,Done" ).ChooseFrom( "D@2" ); // ! This is showing (2) because there are only 2 there.
			user.NextDecision.HasPrompt( "Gather up to (2)" ).HasFromOptions( "D@2,Done" ).ChooseFrom( "D@2" );
			user.NextDecision.HasPrompt( "Gather up to (1)" ).HasFromOptions( "D@2,Done" ).ChooseFrom( "D@2" );
			// Push
			user.NextDecision.HasPrompt( "Push up to (3)" ).HasFromOptions( "D@2,Done" ).ChooseFrom( "D@2" ).HasToOptions( "A5,A6,A7" ).ChooseTo( "A5" );
			user.NextDecision.HasPrompt( "Push up to (2)" ).HasFromOptions( "D@2,Done" ).ChooseFrom( "D@2" ).HasToOptions( "A5,A6,A7" ).ChooseTo( "A5" );
			user.NextDecision.HasPrompt( "Push up to (1)" ).HasFromOptions( "D@2,Done" ).ChooseFrom( "D@2" ).HasToOptions( "A5,A6,A7" ).ChooseTo( "A5" );
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
		Board board = Boards.A;
		GameState gameState = new SoloGameState( spirit, board );
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
			user.NextDecision.HasPrompt( CallToMigrate.Name + ": Target Space" ).HasOptions( "A5,A6,A7,A8" ).Choose( targetSpace );
			//   And: outside dahan are gathered
			user.NextDecision.HasPrompt( "Gather up to (2)" ).HasFromOptions( "D@2,Done" ).ChooseFrom( "D@2" );
			user.NextDecision.HasPrompt( "Gather up to (1)" ).HasFromOptions( "D@2,Done" ).ChooseFrom( "D@2" );

			// Push - 3
			user.NextDecision.HasPrompt( "Push up to (2)" ).HasFromOptions( "D@2,Done" ).ChooseFrom( "D@2" ).HasToOptions( "A5,A6,A7" ).ChooseTo( "A5" );

			user.NextDecision.HasPrompt( "Push up to (2)" ).HasFromOptions( "D@2,Done" ).ChooseFrom( "D@2" )
				.HasToOptions( "A5,A6,A7" ).ChooseTo( "A5" );

			user.NextDecision.HasPrompt( "Push up to (1)" ).HasFromOptions( "D@2,Done" ).ChooseFrom( "D@2" )
				.HasToOptions( "A5,A6,A7" ).ChooseTo( "A5" );

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
		Board board = Boards.A;
		GameState gameState = new SoloGameState( spirit, board );
		Space targetSpace = gameState.Tokens[board[5]];

		// Given: dahan & presence
		targetSpace.InitDefault( Human.Dahan, 1 );
		spirit.Given_IsOn( targetSpace );

		//   And: Played StubbornSolidity
		await Play_StubbornSolidity_On( spirit, targetSpace );

		//  When: Playing card that replaces Dahan - Dissolve the Bonds of Kinship
		await spirit.When_ResolvingCard<DissolveTheBondsOfKinship>((user)=> {
			user.NextDecision.HasPrompt( DissolveTheBondsOfKinship.Name + ": Target Space" ).HasOptions( "A1,A4,A5,A6,A7,A8" ).Choose( targetSpace );
		} );

		//  Then: Dahan are still there (not replaced)
		targetSpace.Summary.ShouldBe( "1D@2,1G,1SUD" ); // 1G => Defend-1 from Stubborn Solidity
	}

	[Trait( "Feature", "Frozen" )]
	[Fact]
	public async Task TargetingDetectsFrozenDahan() {
		Spirit spirit = new StonesUnyieldingDefiance();
		Board board = Boards.A;
		GameState gameState = new SoloGameState( spirit, board );
		Space space = gameState.Tokens[board[5]];

		// Given: dahan & presence
		space.InitDefault( Human.Dahan, 1 );
		spirit.Given_IsOn( space );

		//   And: Played StubbornSolidity
		await Play_StubbornSolidity_On(spirit,space);

		//  When: Playing card that targets a Dahan space CallToTrade
		await using ActionScope uow2 = await ActionScope.StartSpiritAction(ActionCategory.Spirit_Power,spirit);
		Task task = PowerCard.ForDecorated(CallToTrade.ActAsync).ActivateAsync(spirit);

		//  Then: can still target space with frozen dahan
		task.IsCompleted.ShouldBeFalse();
		spirit.NextDecision().HasPrompt("Call to Trade: Target Space").HasOptions("A5").Choose("A5");
	}

	static Task Play_StubbornSolidity_On( Spirit spirit, Space targetSpace ) {
		return StubbornSolidity.ActAsync( spirit.Target( targetSpace.SpaceSpec ) ).ShouldComplete(StubbornSolidity.Name);
	}

}


public class LetThemBreakThemselves_Tests {

	[Fact]
	public async Task IncludesBadlands() {
		// badlands included in damage
		Spirit spirit = new StonesUnyieldingDefiance();
		Board board = Boards.A;
		GameState gameState = new SoloGameState(spirit, board);
		Space space = gameState.Tokens[board[5]];

		// Given: bunch of explorers
		int startingExplorerCount = 6;
		space.InitDefault(Human.Explorer, startingExplorerCount);

		//   And: 1 badlands
		space.Badlands.Init(1);

		//   And: Played 2-damage version of Let-Them-Break-Themselves on space
		await LetThemBreakThemselvesAgainstTheStone.TwoDamage(spirit.Target(space));

		//  When: Ravaging
		await space.Ravage().AwaitUser(user => {
			for(int i=3;i>0;i--)
				user.NextDecision.HasPrompt($"Damage ({i} remaining)").Choose("E@1 on A5");
		});

		//  Then: 2 innate damage + 1 badlands damage = 3 explorers should be removed.
		int removedExplorers = startingExplorerCount - space.Sum(Human.Explorer);
		removedExplorers.ShouldBe(3);
	}

	[Fact]
	public async Task BadlandsUsedTwice() {
		// badlands included in both:
		// * Dahan counter attack (ravage action) AND
		// * Break-Themselves-Against (sub spirit action)
		Spirit spirit = new StonesUnyieldingDefiance();
		Board board = Boards.A;
		GameState gameState = new SoloGameState(spirit, board);
		Space space = gameState.Tokens[board[5]];

		// Given: bunch of explorers
		int startingExplorerCount = 10;
		space.InitDefault(Human.Explorer, startingExplorerCount);
		//   And: 2 dahan
		space.InitDefault(Human.Dahan, 2);
		//   And: enough defend to allow 1 damage to land and dahan
		//        which will trigger + 1 badland damage
		//        resulting in 2 damage total to Dahan killing 1 of them
		space.Init(Token.Defend,startingExplorerCount-1);
		//   And: 1 badlands
		space.Badlands.Init(1);
		//   And: Played 2-damage version of Let-Them-Break-Themselves on space
		await LetThemBreakThemselvesAgainstTheStone.TwoDamage(spirit.Target(space));

		string bob1 = space.ToString();

		//  When: Ravaging
		await space.Ravage().AwaitUser(user => {
			// Then: innate is triggered
			for( int i = 3; i > 0; i-- )
				user.NextDecision.HasPrompt($"Damage ({i} remaining)").Choose("E@1 on A5");
		});

		string bob2 = space.ToString();

		//  Then: 2 innate damage + 1 badlands damage = 3 explorers should be removed.
		int removedExplorers = startingExplorerCount - space.Sum(Human.Explorer);
		const int damageFromDahanCounterAttack = /*dahan*/2 + /*badland*/1;
		const int damageFromInnate             = /*innate*/2 + /*badland*/1;
		removedExplorers.ShouldBe(damageFromDahanCounterAttack + damageFromInnate);
	}
}
