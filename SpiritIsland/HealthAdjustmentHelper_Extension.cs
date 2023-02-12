namespace SpiritIsland;

public static class HealthAdjustmentHelper_Extension {

	public static async Task AdjustTokensHealthForRound( this TargetSpaceCtx ctx, int deltaHealth, params HumanTokenClass[] tokenClasses ) {
		// Doing this on TargetSpaceCtx because adjusting health could destroy a town.

		var tokens = ctx.Tokens;

		// Adjust in land
		await tokens.AdjustHealthOfAll( deltaHealth, tokenClasses ); // ActionScope needed if token is destoryed.

		// Add (covers simple-add AND move-in)
		tokens.Adjust( new TokenAddingHandler( args => {
			if(args.Token is HumanToken healthToken && tokenClasses.Contains( args.Token.Class ))
				args.Token = healthToken.AddHealth( deltaHealth );
		} ), 1 );

		// Remove (covers simple-remove AND move-out)
		var removingToken = new TokenRemovingHandler( async args => {
			if(args.Mode == RemoveMode.Test) return;
			if(args.Token is HumanToken healthToken
				&& tokenClasses.Contains( args.Token.Class )
			)
				// Downgrade the existing tokens health
				// AND change what we are removing to be the downgraded token
				// tokens being destroyed may reduce the count also.
				(args.Token, args.Count) = await tokens.AdjustHealthOf( healthToken, -deltaHealth, args.Count );
		} );
		tokens.Adjust( removingToken, 1 );

		// Add handler to restore ALL at end of round.
		var gameState = ctx.GameState;
		gameState.TimePasses_ThisRound.Push( async gs => {
			// Ensure the dahan are healed before we restore their health.
			gameState.Healer.HealSpace( tokens );
			await tokens.AdjustHealthOfAll( -deltaHealth, tokenClasses );
		} );
	}

}