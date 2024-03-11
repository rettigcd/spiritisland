namespace SpiritIsland.Tests;

internal static class TargetSpaceCtx_ExtensionsForTesting {

	public static void ClearAllBlight( this Spirit _ ) {
		// So it doesn't cascade and require extra interactions...
		foreach(var space in ActionScope.Current.Spaces_Unfiltered)
			space.Blight.Init(0); // don't trigger events
	}

	public static void ActivateFearCard( this Spirit _, IFearCard fearCard ) {
		var fear = GameState.Current.Fear;
		fear.Deck.Pop();
		fear.PushOntoDeck(fearCard);
		fear.Add( fear.PoolMax );
	}

	public static void ElevateTerrorLevelTo( this Spirit _, int desiredFearLevel ) {
		while(GameState.Current.Fear.TerrorLevel < desiredFearLevel)
			GameState.Current.Fear.Deck.Pop();
	}

	#region Log Asserting

	public static void Assert_Ravaged( this Queue<string> log, params string[] spaces ) {
		string action = WaitForNextLogItem( log );
		action.ShouldStartWith( "Ravaging" );
		foreach(var s in spaces)
			WaitForNextLogItem( log ).ShouldStartWith( s );
	}

	static string WaitForNextLogItem( Queue<string> log ) {
		if(log.Count == 0)
			System.Threading.Thread.Sleep(5); // Wait for Engine to catch up
		log.Count.ShouldBeGreaterThan(0,"One of the following issues occured: (a)Didn't wait long enough for invader action to complete. (b)BG thread threw exception, (c)test-condition failed.");
		return log.Dequeue();
	}

	public static void Assert_Built( this Queue<string> log, params string[] spaces ) {

		// If we are skipping the entire card, we don't get the -Build header-
		// if we are skipping individual spaces, then we DO get the header.
		// This might change depending on implementation of 'Skip'

		log.Dequeue().ShouldStartWith( "Building" );
		if(spaces.Length == 0) return; // entire card was skipped, won't see the header message
		foreach(var s in spaces)
			log.Dequeue().ShouldStartWith( s );
		if(spaces.Length == 0)
			log.Dequeue().ShouldStartWith( "No build" );
	}

	public static void Assert_Explored( this Queue<string> log, params string[] spaces ) {
		if(spaces.Length>log.Count)
			throw new System.Exception("Not enough log entries.:" + log.Join(" -- "));

		log.Dequeue().ShouldStartWith( "Exploring" );
		foreach(var s in spaces)
			log.Dequeue().ShouldStartWith( s );
	}

	#endregion

}