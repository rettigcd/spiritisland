namespace SpiritIsland.Tests.Fear;

public class ThrivingCommunities_Tests {

	[Fact]
	public async Task FewerThan4Spaces_DoesntCrash() {

		// on each board:
		// Replace 1 town with a city or Replace 1 explorer with 1 town
		// In 4 different lands with explorer/town,

		var gs = new SoloGameState();

		// Given: board only has 2 spaces with explorers / towns
		SpaceSpec explorerSpace = gs.Board[4];
		SpaceSpec townSpace = gs.Board[5];
		explorerSpace.Given_HasTokens("1E@1");
		townSpace.Given_HasTokens( "1T@2" );

		// When: execute blight card
		await using ActionScope actionScope = await ActionScope.Start(ActionCategory.Blight); // replace generic scope passed in.
		await new ThrivingCommunitites().Immediately.ActAsync( gs )
			.AwaitUser( (user)=> {
				//  And: choose the only 2 spaces
				user.Choose( explorerSpace.Label ); user.Choose( "E@1" );
				user.Choose( townSpace.Label ); user.Choose( "T@2" );
			} ).ShouldComplete("Thriving Communities");

		// Then: spaces should have towns
		gs.Tokens[ explorerSpace ].InvaderSummary().ShouldBe( "1T@2" );
		gs.Tokens[ townSpace ].InvaderSummary().ShouldBe( "1C@3" );

	}

}