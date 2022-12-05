using SpiritIsland.JaggedEarth;

namespace SpiritIsland.Tests;


public class SenseOfDread_Tests {

	[Fact]
	public void Remove1Explorer() {
		var fix = new ConfigurableTestFixture();
		var ravageSpace = fix.GameState.Island.Boards[0][5];
		fix.InitRavageCard(new SpaceSpecificInvaderCard(ravageSpace));

		// Given: ravage space has 2 explorers
		fix.InitTokens(ravageSpace, "2E@1");

		// When: activeate fear card
		var task = new SenseOfDread().Level1( new GameCtx(fix.GameState,ActionCategory.Default ) );

		// And remove 1 explorer from ravage space
		fix.Choose( ravageSpace.Text ); // select ravage space
		fix.Choose( "E@1" ); // remove an explorer

		// Then: there should be 1 explorer left.
		fix.GameState.Tokens[ravageSpace].InvaderSummary().ShouldBe("1E@1");

		task.IsCompletedSuccessfully.ShouldBeTrue();
	}

}

