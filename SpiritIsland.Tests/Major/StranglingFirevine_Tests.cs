namespace SpiritIsland.Tests;

public class StranglingFirevine_Tests {

	[Fact]
	public void SingleAction() {
		var fxt = new ConfigurableTestFixture();

		// Track actions
		HashSet<UnitOfWork> actionScopes = new HashSet<UnitOfWork>();
		fxt.GameState.AddToAllActiveSpaces(new TokenAddedHandler("loggin", x => actionScopes.Add( x.ActionScope ), true ) );
		fxt.GameState.AddToAllActiveSpaces( new TokenRemovedHandler( "loggin", x => actionScopes.Add( x.ActionScope ), true ) );
		fxt.GameState.Tokens.TokenMoved.ForGame.Add( x => actionScopes.Add( x.ActionScope ) );

		// Given: has escalation elements (to make sure we test all parts of this card)
		fxt.InitElements("2 fire,3 plant");

		//   And: target has 2 explorers and 2 towns
		var space = fxt.Board[5];
		fxt.InitTokens(space,"2T@2,2E@1");

		//   And: neighboring Sands has Presence
		fxt.Spirit.Presence.Adjust( fxt.GameState.Tokens[fxt.Board[7]], 1 );

		//  When: activate card
		var ctx = fxt.SelfCtx.Target( space );
		var task = StranglingFirevine.ActAsync( ctx );

		//   And: auto selecting origin land
		//   And: explorers are destoryed

		//   And: apply normal damage
		fxt.Choose( "T@2" );
		fxt.Choose( "T@1" );

		//   And: apply escalation damage
		fxt.Choose( "T@2" );
		fxt.Choose( "T@1" );

		// Then: invaders destroyed, 1 wilds left behind.
		fxt.GameState.Tokens[space].Summary.ShouldBe("1W");

		//  And: both destroys were in a single action
		actionScopes.Count.ShouldBe( 1 );

		//  Then: it is complete and nothing happens.
		task.IsCompleted.ShouldBeTrue();
	}

}
