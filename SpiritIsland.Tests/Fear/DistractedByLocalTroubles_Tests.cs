using SpiritIsland.NatureIncarnate;

namespace SpiritIsland.Tests.Fear;

public class DistractedByLocalTroubles_Tests {

	[Fact]
	public async Task CanKill2ExplorersOnRavageSpace(){

		var gs = new SoloGameState();

		// Given: ravaging on space5
		SpaceSpec ravageSpace = gs.Board[5];
		// fix.InitRavageCard( ravageSpace.BuildInvaderCard() );
		gs.InvaderDeck.Ravage.Cards.Add(ravageSpace.BuildInvaderCard() );


		// Given: ravage space has 2 explorers
		ravageSpace.Given_HasTokens("2E@1");

		var tokens = gs.Tokens[ravageSpace];

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
