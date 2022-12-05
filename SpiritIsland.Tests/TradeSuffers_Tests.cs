namespace SpiritIsland.Tests;

public class TradeSuffers_Tests {

	[Fact]
	public void Level3_DoesntForceCityReplace(){
		// "Each player may replace 1 City with 1 Town or 1 Town with 1 Explorer in a Coastal land." )]
		var fxt = new ConfigurableTestFixture();

		// Given: A1 has a city
		var board = fxt.GameState.Island.Boards[0];
		var space = board[1];
		fxt.InitTokens( space, "1C@3");

		// When: fear card
		var task = new TradeSuffers().Level3( new GameCtx(fxt.GameState, ActionCategory.Default ) );
		//  And: user selects a1
		fxt.Choose( space.Text );
		//  And: user choses not to replace (this is what we are testing...)
		fxt.Choose("Done");

		// Then:
		fxt.GameState.Tokens[space].InvaderSummary().ShouldBe("1C@3");

		task.IsCompletedSuccessfully.ShouldBeTrue();
	}

}
