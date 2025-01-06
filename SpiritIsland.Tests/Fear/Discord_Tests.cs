namespace SpiritIsland.Tests.Fear;

public class Discord_Tests {

	[Trait( "Token", "Strife" )]
	[Trait( "Invader", "Ravage" )]
	[Fact]
	public async Task Level3_StrifedInvadersDamageEachOther() {

		var gs = new SoloGameState();
		// Given: City and Town
		var space = gs.Board[6];
		space.Given_HasTokens( "1C@3,1T@2" );

		// When:
		await new Discord().When_InvokingLevel( 3, (user) => {
			user.NextDecision.HasPrompt( "Select token for Add 1 Strife." )
				.HasOptions( "C@3,T@2" )
				.Choose( "C@3" );
			user.Choose( "T@2" ); // damage town
			user.Choose( "T@1" ); // damage it again.
		} );

		// Then: city destroys 2 explorers, leaving 1
		gs.Tokens[space].InvaderSummary().ShouldBe( "1C@3^" );

	}

}
