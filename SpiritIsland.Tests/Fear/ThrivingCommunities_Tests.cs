namespace SpiritIsland.Tests;

public class ThrivingCommunities_Tests {

	[Fact]
	public void FewerThan4Spaces_DoesntCrash() {

		// on each board:
		// Replace 1 town with a city or Replace 1 explorer with 1 town
		// In 4 different lands with explorer/town,

		var fxt = new ConfigurableTestFixture();
		// Given: board only has 2 spaces with explorers / towns
		Space explorerSpace = fxt.Board[4];
		Space townSpace = fxt.Board[5];
		fxt.InitTokens( explorerSpace, "1E@1" );
		fxt.InitTokens( townSpace, "1T@2" );

		// When: execute blight card
		new ThrivingCommunitites().Immediately.Execute( new GameCtx( fxt.GameState ))
			.FinishUp("Thriving Communities",()=> {
				//  And: choose the only 2 spaces
				fxt.Choose( explorerSpace.Text ); fxt.Choose( "E@1" );
				fxt.Choose( townSpace.Text ); fxt.Choose( "T@2" );
			} );

		// Then: spaces should have towns
		fxt.GameState.Tokens[ explorerSpace ].InvaderSummary().ShouldBe( "1T@2" );
		fxt.GameState.Tokens[ townSpace ].InvaderSummary().ShouldBe( "1C@3" );

	}

}