namespace SpiritIsland.Tests;

public class TigersHunting_Tests {

	[Fact]
	public async Task SingleAction() {
		var fixture = new ConfigurableTestFixture();

		var tracker = new ActionScopeTracker();
		fixture.GameState.AddIslandMod( tracker );
		const int expectedScopeCoung = 1;

		// Given: space 5
		var space = fixture.GameState.Island.Boards[0][5];
		//   And: 1 explorer
		fixture.InitTokens(space,"1E@1");

		//  When: activate card
		var task = TigersHunting.ActAsync( fixture.Spirit.BindMyPowers().Target( space ) );

		// 1 beast is added
		// 1 damage -> destroys explorer
		fixture.Choose("E@1");
		// push up to 2 beasts
		fixture.Choose("Beast"); // 'A' is selecting the beast
		fixture.Choose("A7");

		await task.ShouldComplete();	

		// Then everything was a single action. 
		tracker.Count.ShouldBe(expectedScopeCoung);

	}

}
