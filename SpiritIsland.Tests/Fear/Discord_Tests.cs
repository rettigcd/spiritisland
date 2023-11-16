namespace SpiritIsland.Tests;

public class Discord_Tests {

	[Trait( "Token", "Strife" )]
	[Trait( "Invader", "Ravage" )]
	[Fact]
	public async Task Level3_StrifedInvadersDamageEachOther() {

		var fxt = new ConfigurableTestFixture();
		// Given: City and Town
		var space = fxt.Board[6];
		fxt.InitTokens( space, "1C@3,1T@2" );

		// When:
		await new Discord().When_InvokingLevel( 3, () => {
			fxt.NextDecision.HasPrompt( "Select token for Add 1 Strife." )
				.HasOptions( "C@3,T@2" )
				.Choose( "C@3" );
			fxt.Choose( "T@2" ); // damage town
			fxt.Choose( "T@1" ); // damage it again.
		} );

		// Then: city destroys 2 explorers, leaving 1
		fxt.GameState.Tokens[space].InvaderSummary().ShouldBe( "1C@3^" );

	}

}
