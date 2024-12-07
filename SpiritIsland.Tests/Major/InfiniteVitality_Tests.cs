namespace SpiritIsland.Tests.Major;

public class InfiniteVitality_Tests {

	[Fact]
	public async Task WhenLeavingLand_DamagedDahanAreDestroyed() {
		var gs = new GameConfiguration().ConfigBoards("A").ConfigSpirits(RiverSurges.Name).BuildGame();
		var spirit = gs.Spirits[0];
		var board = gs.Island.Boards[0];
		var a5 = board[5].ScopeSpace;

		// Given: 2 Dahan In Land
		a5.Given_InitSummary("2D@2");

		//   And: Infinite Vitality played on land
		var ctx = spirit.Target(a5);
		await InfiniteVitality.ActAsync(ctx);
		a5.Summary.ShouldBe("2D@6");
		var dahan6Token = a5.HumanOfTag(Human.Dahan).Single();
		dahan6Token.FullHealth.ShouldBe(6);

		//   And: 1 dahan receives 2 damage (like in Ravage)
		await a5.Dahan.ApplyDamageToAll_Efficiently(2, dahan6Token);
		a5.Summary.ShouldBe("1D@4,1D@6");

		//  When: push the damaged dahan out of the space
		await ctx.PushDahan(1).AwaitUser(u => {
			u.NextDecision.HasPrompt("Push (1)").Choose("D@4 on A5 => A1");
		});

		//  Then: no exception
	}
}