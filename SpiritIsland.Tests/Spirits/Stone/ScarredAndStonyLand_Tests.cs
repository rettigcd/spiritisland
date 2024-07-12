namespace SpiritIsland.Tests.Spirits.StoneNS;

public class ScarredAndStonyLand_Tests {
	
	[Fact]
	public async Task RemovedBlight_GoesToBag() {
		// blight DOES NOT goes back to card
		Spirit spirit = new StonesUnyieldingDefiance();
		Board board = Board.BuildBoardC();
		GameState gameState = new GameState(spirit, board);
		Space targetSpace = gameState.Tokens[board[5]];

		// Given: 1 blight on space
		targetSpace.Blight.Init(1);
		// Some blight on card
		int originalCardBlight = gameState.BlightCard.BlightCount;

		//  When: Playing card that Gathers and Pushes - Call to Migrate
		await ScarredAndStonyLand.ActAsync(spirit.Target(targetSpace));

		//  Then: Blight returned to card = 0
		int blightReturnedToCard = gameState.BlightCard.BlightCount - originalCardBlight;
		blightReturnedToCard.ShouldBe(0);

		//   And: just for fun, test the blight is removed from space
		targetSpace.Blight.Count.ShouldBe(0);
	}

	[Fact]
	public async Task IncludesDamageFromBadlands() {
		Spirit spirit = new StonesUnyieldingDefiance();
		Board board = Board.BuildBoardC();
		GameState gameState = new GameState(spirit, board);
		Space targetSpace = gameState.Tokens[board[5]];

		// Given: 1 blight on space
		targetSpace.Blight.Init(1);
		//   And: Some explorers
		const int originalExplorerCount = 6;
		targetSpace.InitDefault(Human.Explorer,originalExplorerCount);
		//   And: 1 badlands
		targetSpace.Badlands.Init(1);

		//  When: Playing card that Gathers and Pushes - Call to Migrate
		await ScarredAndStonyLand.ActAsync(spirit.Target(targetSpace)).AwaitUser(user => {
			// Then 3 explorers are removed
			for(int i = 3; i > 0; i-- )
				user.NextDecision.HasPrompt($"Damage ({i} remaining)").HasOptions("E@1 on C5").Choose("E@1 on C5");
		});

		//  Then: 3 Explorers are removed
		targetSpace.SumAny(Human.Explorer).ShouldBe(originalExplorerCount-3);
	}

}

