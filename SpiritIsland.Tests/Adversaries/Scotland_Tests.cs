namespace SpiritIsland.Tests.Adversaries;

public class Scotland_Tests {

	[Fact]
	public void TwoCoastalCities_IsNotLoss() {

		// Given: Russia, Level 0
		GameConfiguration cfg = Given_ScotlandLevel(0).ConfigBoards("A").ConfigSpirits(RiverSurges.Name);
		GameState gameState = BuildGame(cfg);
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

		// Given: Russia, Level 0
		GameConfiguration cfg = Given_ScotlandLevel(0).ConfigBoards("A").ConfigSpirits(RiverSurges.Name);
		GameState gameState = BuildGame(cfg);
		Board board = gameState.Island.Boards[0];

		//   And: Cities on 2 coastal spaces
		board[1].ScopeSpace.InitDefault(Human.City, 1);
		board[2].ScopeSpace.InitDefault(Human.City, 1);
		board[3].ScopeSpace.InitDefault(Human.City, 1);

		// When: check for loss condition
		var goex = Should.Throw<GameOverException>(gameState.CheckWinLoss);
		goex.Status.Result.ShouldBe(GameOverResult.Defeat);
	}


	static GameConfiguration Given_ScotlandLevel(int level) => new GameConfiguration { Adversary = new AdversaryConfig(Scotland.Name, level), ShuffleNumber = 1, };
	static GameState BuildGame(GameConfiguration cfg) => ConfigurableTestFixture.GameBuilder.BuildGame(cfg);

}
