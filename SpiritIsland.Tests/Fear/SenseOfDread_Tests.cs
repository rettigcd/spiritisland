namespace SpiritIsland.Tests.Fear;

public class SenseOfDread_Tests {

	[Fact]
	public async Task Remove1Explorer() {
		var fix = new ConfigurableTestFixture();
		var ravageSpace = fix.GameState.Island.Boards[0][5];
		fix.InitRavageCard( ravageSpace.BuildInvaderCard() );

		// Given: ravage space has 2 explorers
		fix.InitTokens(ravageSpace, "2E@1");

		// When: activate fear card
		await new SenseOfDread().When_InvokingLevel(1, () => {
			// And remove 1 explorer from ravage space
			fix.Choose( ravageSpace.Text ); // select ravage space
			fix.Choose( "E@1" ); // remove an explorer
		} );

		// Then: there should be 1 explorer left.
		fix.GameState.Tokens[ravageSpace].InvaderSummary().ShouldBe("1E@1");

	}

	[Fact]
	public async Task Remove1Town() {
		var fix = new ConfigurableTestFixture();
		var ravageSpace = fix.GameState.Island.Boards[0][5];
		fix.InitRavageCard( ravageSpace.BuildInvaderCard() );

		// Given: ravage space has 1 explorer, 1 town
		fix.InitTokens(ravageSpace, "1E@1,1T@2");

		// When: activate fear card
		await new SenseOfDread().When_InvokingLevel(2, () => {
			// And remove 1 explorer from ravage space
			fix.Choose( ravageSpace.Text ); // select ravage space
			fix.Choose( "T@2" ); // remove an explorer
		} );

		// Then: there should be 1 explorer left.
		fix.GameState.Tokens[ravageSpace].InvaderSummary().ShouldBe("1E@1");

	}


}

