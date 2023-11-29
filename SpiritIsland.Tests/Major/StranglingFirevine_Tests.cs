namespace SpiritIsland.Tests;

public class StranglingFirevine_Tests {

	[Fact]
	public async Task SingleAction() {
		var fxt = new ConfigurableTestFixture();

		// Track actions
		var tracker = new ActionScopeTracker();
		fxt.GameState.AddIslandMod( tracker );

		// Given: has escalation elements (to make sure we test all parts of this card)
		fxt.InitElements("2 fire,3 plant");

		//   And: target has 2 explorers and 2 towns
		var space = fxt.Board[5];
		fxt.InitTokens(space,"2T@2,2E@1");

		//   And: neighboring Sands has Presence
		SpiritExtensions.Given_Adjust( fxt.Spirit.Presence, fxt.GameState.Tokens[fxt.Board[7]], 1 );

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

		await task.ShouldComplete();

		// Then: invaders destroyed, 1 wilds left behind.
		fxt.GameState.Tokens[space].Summary.ShouldBe("1W");

		//  And: both destroys were in a single action
		tracker.Count.ShouldBe( 1 );

	}

}
public class ActionScopeTracker : BaseModEntity, IHandleTokenAdded, IHandleTokenRemoved {

	readonly HashSet<ActionScope> _scopes = new();
	public int Count => _scopes.Count;

	void IHandleTokenAdded.HandleTokenAdded( ITokenAddedArgs _ ) 
		=> _scopes.Add( ActionScope.Current );

	void IHandleTokenRemoved.HandleTokenRemoved(SpiritIsland.ITokenRemovedArgs _) 
		=> _scopes.Add( ActionScope.Current );

}
