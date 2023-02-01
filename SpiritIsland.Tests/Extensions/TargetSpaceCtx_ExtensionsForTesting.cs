namespace SpiritIsland.Tests;

internal static class TargetSpaceCtx_ExtensionsForTesting {

	public static void Init( this SpaceState currentTokens, string expectedInvaderSummary ) {

		CountDictionary<IToken> desiredTokens = new();
		if(!string.IsNullOrEmpty( expectedInvaderSummary )) { 
			foreach(var part in expectedInvaderSummary.Split( ',' )) {
				IToken token = part[1..] switch {
					"E@1" => StdTokens.Explorer,
					"T@2" => StdTokens.Town,
					"C@3" => StdTokens.City,
					"D@2" => StdTokens.Dahan,
					"Z" => Token.Disease,
					_ => throw new ArgumentException("invalide tokentype found in "+expectedInvaderSummary)
				};
				desiredTokens.Add(token, int.Parse(part[..1] ) );
			}
		}

		var tokensToRemove = currentTokens.Keys.Except(desiredTokens.Keys).ToArray();
		foreach(var old in tokensToRemove)
			currentTokens.Init(old,0);
		foreach(var p in desiredTokens)
			currentTokens.Init(p.Key,p.Value);

		currentTokens.Summary.ShouldBe( expectedInvaderSummary == "" ? "[none]" : expectedInvaderSummary );
	}

	public static void ClearAllBlight( this SelfCtx ctx ) {
		// So it doesn't cascade and require extra interactions...
		foreach(var space in ctx.GameState.AllSpaces)
			space.Blight.Init(0); // don't trigger events
	}

	public static void ActivateFearCard( this SelfCtx ctx, IFearCard fearCard ) {
		var fear = ctx.GameState.Fear;
		fear.Deck.Pop();
		fear.PushOntoDeck(fearCard);
		fear.AddDirect( new FearArgs( fear.PoolMax ) );
	}

	public static void ElevateTerrorLevelTo( this SelfCtx ctx, int desiredFearLevel ) {
		while(ctx.GameState.Fear.TerrorLevel < desiredFearLevel)
			ctx.GameState.Fear.Deck.Pop();
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