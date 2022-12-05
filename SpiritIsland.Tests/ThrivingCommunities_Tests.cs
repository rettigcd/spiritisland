using SpiritIsland.JaggedEarth;

namespace SpiritIsland.Tests;

public class ThrivingCommunities_Tests {

	[Fact]
	public void FewerThan4Spaces_DoesntCrash() {

		// on each board:
		// Replace 1 town with a city or Replace 1 explorer with 1 town
		// In 4 different lands with explorer/town,

		var fxt = new ConfigurableTestFixture();
		// Given: board only has 2 spaces with explorers / towns
		var space1 = fxt.Board[4];
		var space2 = fxt.Board[5];
		fxt.InitTokens( space1, "1E@1" );
		fxt.InitTokens( space2, "1T@2" );

		// When: execute blight card
		var task = new ThrivingCommunitites().Immediately.Execute( new GameCtx( fxt.GameState, ActionCategory.Blight ));
		//  And: choose the only 2 spaces
		fxt.Choose( space1.Text ); fxt.Choose( "E@1" );
		fxt.Choose( space2.Text ); fxt.Choose( "T@2" );

		// Then: it should complete successfully
		task.IsCompletedSuccessfully.ShouldBeTrue();
		//  And: spaces should have towns
		fxt.GameState.Tokens[ space1 ].InvaderSummary().ShouldBe( "1T@2" );
		fxt.GameState.Tokens[ space2 ].InvaderSummary().ShouldBe( "1C@3" );

	}

}