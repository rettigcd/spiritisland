namespace SpiritIsland.Tests.Major;

public class StranglingFirevine_Tests {

	[Fact]
	public async Task SingleAction() {
		var gs = new SoloGameState();

		// Track actions
		var tracker = new ActionScopeTracker();
		gs.AddIslandMod( tracker );

		// Given: has escalation elements (to make sure we test all parts of this card)
		gs.Spirit.Configure().Elements("2 fire,3 plant");

		//   And: target has 2 explorers and 2 towns
		var space = gs.Board[5];
		space.Given_HasTokens("2T@2,2E@1");

		//   And: neighboring Sands has Presence
		gs.Spirit.Given_IsOn( gs.Tokens[gs.Board[7]] );

		//  When: activate card
		await gs.Spirit.When_ResolvingCard<StranglingFirevine>(user=>{
			user.NextDecision.HasPrompt("Strangling Firevine: Target Space").HasOptions("A5,A7,A8").Choose("A5");
			// Accept threshold
			user.NextDecision.Choose("Yes");
			//   And: apply normal damage
			user.NextDecision.Choose("T@2");
			user.NextDecision.Choose("T@1");
			//   And: apply escalation damage
			user.NextDecision.Choose("T@2");
			user.NextDecision.Choose("T@1");
		});

		// Then: invaders destroyed, 1 wilds left behind.
		gs.Tokens[space].Summary.ShouldBe("1W");

		//  And: both destroys were in a single action
		tracker.Count.ShouldBe( 1 );

	}

}
public class ActionScopeTracker : BaseModEntity, IHandleTokenAdded, IHandleTokenRemoved {

	readonly HashSet<ActionScope> _scopes = [];
	public int Count => _scopes.Count;

	Task IHandleTokenAdded.HandleTokenAddedAsync( Space _, ITokenAddedArgs _1) {
		_scopes.Add( ActionScope.Current );
		return Task.CompletedTask;
	}

	Task IHandleTokenRemoved.HandleTokenRemovedAsync( ITokenRemovedArgs _ ) {
		_scopes.Add(ActionScope.Current);
		return Task.CompletedTask;
	}

}
