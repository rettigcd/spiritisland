namespace SpiritIsland.Tests.Minor;

public class CallToTrade_Tests {

	[Trait("Invaders", "Ravage")]
	[Trait("Invaders", "Build")]
	[Trait("Invaders", "Deck")]
	[Trait("Feature", "Gather")]
	[Theory]
	[InlineData(0, 0, 1, "2D@2,3E@1")]           // card does not manufacture builds out of thin air
	[InlineData(1, 0, 1, "2D@2,3E@1,1T@2")]      // 1 Ravage => becomes 1 Build
	[InlineData(1, 1, 1, "1C@3,2D@2,3E@1,1T@2")] // Build and Ravage => 2 Builds
	[InlineData(2, 0, 1, "1B,1D@1,1E@1,1T@2")]   // 2 Ravages => 1 Ravage 1 Build
	[InlineData(1, 0, 3, "1B,1D@1,1E@1")]     // Terror Level 3 => Ravage Remains Ravage
	public async Task ConvertRavagesToBuilds(int ravageCount, int buildCount, int terrorLevel, string expectedEndingTokens) {

		var gs = new SoloGameState(Boards.A);
		var jungleSpace = gs.Board[8].ScopeSpace; // jungle

		// Given: a space with an 3 explorer and 2 dahan
		jungleSpace.Given_InitSummary("2D@2,3E@1");

		//   And: terror Level is...
		while( gs.Fear.TerrorLevel < terrorLevel ) gs.Fear.Deck.Pop();

		//   And: Call to Trade Played played
		await CallToTrade.ActAsync(gs.Spirit.Target(jungleSpace));

		//   And: Init Ravage cards
		while(0 < ravageCount-- ) gs.InvaderDeck.Ravage.Cards.Add(InvaderCard.Stage1(Terrain.Jungle));

		//   And: init Build cards
		if( buildCount == 0 ) gs.InvaderDeck.Build.Cards.Add(InvaderCard.Stage1(Terrain.Wetland));
		// !!! Above line is required due to a BUG. - Having a card is required to detect the Build Tokens.  
		// !!! Currently Build step with no cards will never scan for build Tokens.

		while( 0 < buildCount-- ) gs.InvaderDeck.Build.Cards.Add(InvaderCard.Stage1(Terrain.Jungle));

		//  When: Invader Actions occur.
		await InvaderPhase.ActAsync(gs);

		//  Then: Get Expected
		jungleSpace.Summary.ShouldBe(expectedEndingTokens);
	}

}