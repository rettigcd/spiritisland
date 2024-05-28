namespace SpiritIsland.Tests.Fear;

public class ThrivingCommunities_Tests {

	[Fact]
	public async Task FewerThan4Spaces_DoesntCrash() {

		// on each board:
		// Replace 1 town with a city or Replace 1 explorer with 1 town
		// In 4 different lands with explorer/town,

		var fxt = new ConfigurableTestFixture();
		// Given: board only has 2 spaces with explorers / towns
		SpaceSpec explorerSpace = fxt.Board[4];
		SpaceSpec townSpace = fxt.Board[5];
		fxt.InitTokens( explorerSpace, "1E@1" );
		fxt.InitTokens( townSpace, "1T@2" );

		// When: execute blight card
		await using ActionScope actionScope = await ActionScope.Start(ActionCategory.Blight); // replace generic scope passed in.
		await new ThrivingCommunitites().Immediately.ActAsync( fxt.GameState )
			.AwaitUser( (user)=> {
				//  And: choose the only 2 spaces
				user.Choose( explorerSpace.Label ); fxt.Choose( "E@1" );
				user.Choose( townSpace.Label ); fxt.Choose( "T@2" );
			} ).ShouldComplete("Thriving Communities");

		// Then: spaces should have towns
		fxt.GameState.Tokens[ explorerSpace ].InvaderSummary().ShouldBe( "1T@2" );
		fxt.GameState.Tokens[ townSpace ].InvaderSummary().ShouldBe( "1C@3" );

	}

}