namespace SpiritIsland.Tests.Minor;

public class VeilTheNightsHunt_Tests {

	[Fact]
	public async Task SoloInvader_IsOnlyDamagedOnce() {
		var board = Boards.A;
		var spirit = new RiverSurges();
		var gs = new GameState(spirit, board);
		var targetSpace = board[5].ScopeSpace;

		// Given: 2 dahan & 1 town  (2 dahan would normally be able to damage 2 different items)
		targetSpace.Given_InitSummary("2D@2,1T@2");

		//  When: playing Card
		await VeilTheNightsHunt.Act(spirit.Target(targetSpace)).AwaitUser(user => {
			// Select: deal damage
			user.NextDecision.HasPrompt("Select Power Option").Choose("Each dahan deals 1 damage to a different invader");

			// And: Deal 1 damage to the solo invader
			user.NextDecision.HasPrompt("Select invader to apply 1 damage").Choose("T@2 on A5");

			// Then: we should be done, even though there are 2 dahan
		}).ShouldComplete();

	}

}