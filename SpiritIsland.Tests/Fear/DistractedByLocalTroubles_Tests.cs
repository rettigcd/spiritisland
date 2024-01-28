using System;
using SpiritIsland.NatureIncarnate;

namespace SpiritIsland.Tests.Fear;

public class DistractedByLocalTroubles_Tests {

	[Fact]
	public async Task CanKill2Explorers(){

		var fix = new ConfigurableTestFixture();
		Space ravageSpace = fix.GameState.Island.Boards[0][5];
		fix.InitRavageCard( ravageSpace.BuildInvaderCard() );

		// Given: ravage space has 2 explorers
		fix.InitTokens(ravageSpace, "2E@1");

		var tokens = fix.GameState.Tokens[ravageSpace];

		// When: activeate fear card - level 2
		await new DistractedByLocalTroubles().When_InvokingLevel(2, (user) => {
			user.NextDecision.HasPrompt("Select space to 1 Damage each to up to 2 Invaders").HasOptions("A2,A5").Choose("A5");
			user.NextDecision.HasPrompt("Damage-1 each (2 remaining)").HasOptions("E@1 on A5,Done").Choose( "E@1" );
			user.NextDecision.HasPrompt("Damage-1 each (1 remaining)").HasOptions("E@1 on A5,Done").Choose( "E@1" );
		} );

		// Then: none left
		tokens.InvaderSummary().ShouldBe("");

	}

}
