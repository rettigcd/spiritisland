namespace SpiritIsland.Tests;

public class SenseOfDread_Tests {

	[Fact]
	public async Task Remove1Explorer() {
		var fix = new ConfigurableTestFixture();
		var ravageSpace = fix.GameState.Island.Boards[0][5];
		fix.InitRavageCard( ravageSpace.BuildInvaderCard() );

		// Given: ravage space has 2 explorers
		fix.InitTokens(ravageSpace, "2E@1");

		// When: activeate fear card
		await using var scope = fix.GameState.StartAction(ActionCategory.Invader);
		var task = new SenseOfDread().Level1( new GameCtx(fix.GameState,scope ) );

		// And remove 1 explorer from ravage space
		fix.Choose( ravageSpace.Text ); // select ravage space
		fix.Choose( "E@1" ); // remove an explorer

		// Then: there should be 1 explorer left.
		fix.GameState.Tokens[ravageSpace].InvaderSummary().ShouldBe("1E@1");

		task.IsCompletedSuccessfully.ShouldBeTrue();
	}

}

