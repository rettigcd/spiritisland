namespace SpiritIsland.Tests.Adversaries;

public class Scotland_Tests {

	[Fact]
	public void TwoCoastalCities_IsNotLoss() {

		// Given: Russia, Level 0
		GameState gameState = Given_ScotlandSoloGame(0);
		Board board = gameState.Island.Boards[0];

		//   And: Cities on 2 coastal spaces
		board[1].ScopeSpace.InitDefault(Human.City,1);
		board[2].ScopeSpace.InitDefault(Human.City, 1);
		board[3].ScopeSpace.InitDefault(Human.City, 0);

		// When: check for loss condition
		Should.NotThrow( gameState.CheckWinLoss );
	}

	[Fact]
	public void ThreeCoastalCities_IsLoss() {

		// Given: Scotland, Level 0
		GameState gameState = Given_ScotlandSoloGame(0);
		Board board = gameState.Island.Boards[0];

		//   And: Cities on 2 coastal spaces
		board[1].ScopeSpace.InitDefault(Human.City, 1);
		board[2].ScopeSpace.InitDefault(Human.City, 1);
		board[3].ScopeSpace.InitDefault(Human.City, 1);

		// When: check for loss condition
		var goex = Should.Throw<GameOverException>(gameState.CheckWinLoss);
		goex.Status.Result.ShouldBe(GameOverResult.Defeat);
	}

	[Theory]
	[InlineData(false, false, true)]
	[InlineData(true, false, false)]
	[InlineData(false, true, false)]
	public async Task EmptyCoastalLandsAdjacentToCity_Builds(bool buildSpaceIsolated, bool cityIsolated, bool doesBuild) {
		// Given: Scotland, Level 3
		GameState gameState = Given_ScotlandSoloGame(3);
		Board board = gameState.Island.Boards[0];
		Space a1 = board[1].ScopeSpace;
		Space a2 = board[2].ScopeSpace;

		//   And: Land 1 is empty
		a1.Given_ClearTokens();
		//   And: has an adjacent city (in Land 2)
		a2.Given_InitSummary("1C@3");

		//   And: Land 1 is (conditionally isolated)
		if(buildSpaceIsolated) a1.Isolate();

		//   And: Land 2 is (conditionally isolated)
		if( cityIsolated ) a2.Isolate();

		// When: building
		var mountainCard = InvaderDeckBuilder.Level1Cards.Single(c => c.Code == "M");
		await gameState.InvaderDeck.Build.ActivateCard(mountainCard, gameState);

		// Then: there should/should no be a Town on A1
		a1.Assert_HasInvaders(doesBuild ? "1T@2" : "");
	}

	static GameState Given_ScotlandSoloGame(int level) => new GameConfiguration()
		.ConfigAdversary(Scotland.Name, level)
		.ConfigBoards("A")
		.ConfigSpirits(RiverSurges.Name)
		.BuildGame();

}
