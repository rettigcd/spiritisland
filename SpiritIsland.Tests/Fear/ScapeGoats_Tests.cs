namespace SpiritIsland.Tests;

public class ScapeGoats_Tests {

	[Trait("Invaders","Ravage")]
	[Fact]
	public async Task Level2_StrifedInvadersDoDamage() {

		var fxt = new ConfigurableTestFixture();
		// Given: Strifed City with 3 explorers
		var citySpace = fxt.Board[6];
		fxt.InitTokens(citySpace,"1C@3^,3E@1");

		// Given: Strifed Town with 3 explorers
		var townSpace = fxt.Board[4];
		fxt.InitTokens( townSpace, "1T@2^,3E@1" );

		// When:
		await new Scapegoats().When_InvokingLevel( 2 );

		// Then: city destroys 2 explorers, leaving 1
		fxt.GameState.Tokens[citySpace].InvaderSummary().ShouldBe("1C@3^,1E@1");

		//  And: town destroys 1 explorer, leaving 2
		fxt.GameState.Tokens[townSpace].InvaderSummary().ShouldBe( "1T@2^,2E@1" );

	}

}
