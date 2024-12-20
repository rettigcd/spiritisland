namespace SpiritIsland.Tests.Major;

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
		await TigersHunting.ActAsync( fixture.Spirit.Target( space ) ).AwaitUser( user => {
			// 1 beast is added
			// 1 damage -> destroys explorer
			user.Choose("E@1");
			// push up to 2 beasts
			user.NextDecision.HasPrompt("Push up to (1)")
				.HasFromOptions("Beast,Done").ChooseFrom("Beast")
				.HasToOptions("A1,A4,A6,A7,A8").ChooseTo("A7");
		}).ShouldComplete();	

		// Then everything was a single action. 
		tracker.Count.ShouldBe(expectedScopeCoung);

	}

}
