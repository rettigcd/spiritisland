namespace SpiritIsland.Tests.Spirits.StoneNS;

public class BestoreTheEnduranceOfBedRock_Tests {
	[Fact]
	public async Task BlightOutnumbersPresence_CascadesAndDestroysPresence() {
		// too few presences => Normal cascade and destory presence
		Spirit spirit = new StonesUnyieldingDefiance();
		Board board = Boards.C;
		GameState gameState = new SoloGameState(spirit, board);
		Space targetSpace = gameState.Tokens[board[5]];

		// Setup: prevent game-over loss
		spirit.Given_IsOn(gameState.Tokens[board[1]], 1); // so spirit has other presence on board
		gameState.BlightCard.InitBlight(10); // don't let island blight

		// Given: starting blight matches starting presence on space
		targetSpace.Blight.Init(1);
		spirit.Given_IsOn(targetSpace, 1);

		//  When: adding blight (blight NOW outnumbers presence)
		await DireMetamorphosis.ActAsync(spirit.Target(targetSpace)).AwaitUser(user => {
			// Then Blight cascades
			user.NextDecision.HasPrompt("Cascade blight from C5 to").Choose("C6");
		});

		//  And: presence is destroyed
		spirit.Presence.IsOn(targetSpace).ShouldBeFalse();
	}

	[Fact]
	public async Task PresenceMatchesOrExceedsBlight_NoDestroyNorCascade() {
		// too few presences => Normal cascade and destory presence
		Spirit spirit = new StonesUnyieldingDefiance();
		Board board = Boards.C;
		GameState gameState = new SoloGameState(spirit, board);
		Space targetSpace = gameState.Tokens[board[5]];

		// Given: starting blight is 1 less than presence
		targetSpace.Blight.Init(0);
		spirit.Given_IsOn(targetSpace, 1);

		//  When: adding blight
		await DireMetamorphosis.ActAsync(spirit.Target(targetSpace)).AwaitUser(user => {
			// Then: No Cascade decision occurs
			// user.NextDecision.HasPrompt("Cascade blight from C5 to").Choose("C6");
		});

		//  And: presence remains
		spirit.Presence.IsOn(targetSpace).ShouldBeTrue();
	}
}

