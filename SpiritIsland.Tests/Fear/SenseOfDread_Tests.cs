namespace SpiritIsland.Tests.Fear;

public class SenseOfDread_Tests {

	[Fact]
	public async Task Remove1Explorer() {
		var gs = new SoloGameState();
		var ravageSpace = gs.Island.Boards[0][5];
		// fix.InitRavageCard( ravageSpace.BuildInvaderCard() );
		gs.InvaderDeck.Ravage.Cards.Add(ravageSpace.BuildInvaderCard());

		// Given: ravage space has 2 explorers
		ravageSpace.Given_HasTokens("2E@1");

		// When: activate fear card
		await new SenseOfDread().When_InvokingLevel(1, (user) => {
			// And remove 1 explorer from ravage space
			user.Choose( ravageSpace.Label ); // select ravage space
			user.Choose( "E@1" ); // remove an explorer
		} );

		// Then: there should be 1 explorer left.
		gs.Tokens[ravageSpace].InvaderSummary().ShouldBe("1E@1");

	}

	[Fact]
	public async Task Remove1Town() {
		var gs = new SoloGameState();
		var ravageSpace = gs.Island.Boards[0][5];
		// fix.InitRavageCard( ravageSpace.BuildInvaderCard() );
		gs.InvaderDeck.Ravage.Cards.Add( ravageSpace.BuildInvaderCard() );

		// Given: ravage space has 1 explorer, 1 town
		ravageSpace.Given_HasTokens("1E@1,1T@2");

		// When: activate fear card
		await new SenseOfDread().When_InvokingLevel(2, (user) => {
			// And remove 1 explorer from ravage space
			user.Choose( ravageSpace.Label ); // select ravage space
			user.Choose( "T@2" ); // remove an explorer
		} );

		// Then: there should be 1 explorer left.
		gs.Tokens[ravageSpace].InvaderSummary().ShouldBe("1E@1");

	}


}

