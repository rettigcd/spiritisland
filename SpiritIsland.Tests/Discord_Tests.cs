namespace SpiritIsland.Tests;

public class Discord_Tests {

	[Fact]
	public void Level3_StrifedInvadersDamageEachOther() {

		var fxt = new ConfigurableTestFixture();
		// Given: City and Town
		var space = fxt.Board[6];
		fxt.InitTokens( space, "1C@3,1T@2" );

		// When:
		var task = new Discord().Level3( new GameCtx( fxt.GameState, ActionCategory.Fear ) );

		fxt.Choose(space.Text); // select space to add strife
		fxt.Choose("C@3"); // strife the city
		
		fxt.Choose("T@2"); // damage town
		fxt.Choose("T@1"); // damage it again.

		// Then: city destroys 2 explorers, leaving 1
		fxt.GameState.Tokens[space].InvaderSummary().ShouldBe( "1C@3^" );

		task.IsCompletedSuccessfully.ShouldBeTrue();
	}

}
