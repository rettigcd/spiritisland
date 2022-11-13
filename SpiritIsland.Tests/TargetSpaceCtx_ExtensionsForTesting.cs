namespace SpiritIsland.Tests;

internal static class TargetSpaceCtx_ExtensionsForTesting {

	public static void Init( this SpaceState currentTokens, string expectedInvaderSummary ) {

		CountDictionary<Token> desiredTokens = new();
		if(!string.IsNullOrEmpty( expectedInvaderSummary )) { 
			foreach(var part in expectedInvaderSummary.Split( ',' )) {
				Token token = part[1..] switch {
					"E@1" => StdTokens.Explorer,
					"T@2" => StdTokens.Town,
					"C@3" => StdTokens.City,
					"D@2" => StdTokens.Dahan,
					"Z" => TokenType.Disease,
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
		foreach(var space in ctx.AllSpaces) {
			var tmpCtx = ctx.Target(space);
			tmpCtx.Tokens.Blight.Init(0); // don't trigger events
		}
	}

	public static void ActivateFearCard( this SelfCtx ctx, IFearOptions fearCard ) {
		ctx.GameState.Fear.Deck.Pop();
		ctx.GameState.Fear.ActivatedCards.Push( new PositionFearCard{ FearOptions=fearCard, Text="FearCard" } );
	}

	public static void ElevateTerrorLevelTo( this SelfCtx ctx, int desiredFearLevel ) {
		while(ctx.GameState.Fear.TerrorLevel < desiredFearLevel)
			ctx.GameState.Fear.Deck.Pop();
	}

	#region Log Asserting

	public static void Assert_Ravaged( this Queue<string> log, params string[] spaces ) {

		var action = log.Dequeue();
		action.ShouldStartWith( "Ravaging" );
		foreach(var s in spaces)
			log.Dequeue().ShouldStartWith( s );
	}

	public static void Assert_Built( this Queue<string> log, params string[] spaces ) {

		// If we are skipping the entire card, we don't get the -Build header-
		// if we are skipping individual spaces, then we DO get the header.
		// This might change depending on implementation of 'Skip'

		log.Dequeue().ShouldStartWith( "Building" );
		if(spaces.Length == 0) return; // entire card was skipped, won't see the header message
		foreach(var s in spaces)
			log.Dequeue().ShouldStartWith( s );
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