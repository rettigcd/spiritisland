namespace SpiritIsland.Tests;

static class InvaderCard_Extensions {

	internal static void When_Ravaging(this InvaderCard invaderCard, Action userActions = null) {
		Task task = new RavageEngine().ActivateCard( invaderCard, GameState.Current );
		task.FinishUp("Ravage",userActions);
	}

	internal static void When_Building( this InvaderCard invaderCard ) {
		Task t = new BuildEngine().ActivateCard( invaderCard, GameState.Current );
		t.FinishUp("Build");
	}

	internal static void When_Exploring( this InvaderCard invaderCard ) {
		Task t = new ExploreEngine().ActivateCard( invaderCard, GameState.Current );
		t.FinishUp("Explore");
	}

	internal static void FinishUp(this Task task, string taskDescription, Action userActions = null ) {
		userActions?.Invoke();
		task.Wait( 3000 );
		if(!task.IsCompletedSuccessfully)
			throw new Exception( $"{taskDescription} did not complete in a reasonable time." );
	}


	internal static void When_InvokingLevel( this IFearCard card, int level, Action userActions=null ) {
		var ctx = new GameCtx( GameState.Current );
		(level switch {
			1 => card.Level1( ctx ),
			2 => card.Level2( ctx ),
			3 => card.Level3( ctx ),
			_ => throw new ArgumentOutOfRangeException(nameof(level))
		}).FinishUp( $"{card.GetType().Name}-{level}", userActions );
	}


}
