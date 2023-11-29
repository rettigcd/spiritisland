namespace SpiritIsland.Tests;

public class TigersHunting_Tests {

	[Fact]
	public async Task SingleAction() {
		var fixture = new ConfigurableTestFixture();

		var tracker = new ActionScopeTracker();
		fixture.GameState.AddIslandMod( tracker );

		// Given: space 5
		var space = fixture.GameState.Island.Boards[0][5];
		//   And: 1 explorer
		fixture.InitTokens(space,"1E@1");

		//  When: activate card
		var ctx = fixture.SelfCtx.Target( space );
		var task = TigersHunting.ActAsync( ctx );

		// 1 beast is added
		tracker.Count.ShouldBe(1);

		// 1 damage -> destroys explorer
		fixture.Choose("E@1");
		tracker.Count.ShouldBe( 1 );

		// push up to 2 beasts
		fixture.Choose("Beast"); // 'A' is selecting the beast
		fixture.Choose("A7");

		await task.ShouldComplete();	

		// Then everything was a single action. 
		tracker.Count.ShouldBe(1);

	}

}
