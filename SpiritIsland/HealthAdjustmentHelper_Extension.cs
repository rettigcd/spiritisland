namespace SpiritIsland;

public static class HealthAdjustmentHelper_Extension {

	public static async Task AdjustTokensHealthForRound( this TargetSpaceCtx ctx, int deltaHealth, params HealthTokenClass[] tokenClasses ) {

		// Adjust in land
		await ctx.Tokens.AdjustHealthOfAll( deltaHealth, ctx.ActionCtx, tokenClasses ); // ActionID needed if token is destoryed.

		// Add (covers simple-add AND move-in)
		ctx.Tokens.Adjust( new TokenAddingHandler( args => {
			if(args.Token is HealthToken healthToken && tokenClasses.Contains( args.Token.Class ))
				args.Token = healthToken.AddHealth( deltaHealth );
		} ), 1 );

		// Remove (covers simple-remove AND move-out)
		var removingToken = new TokenRemovingHandler( async args => {
			if(args.Token is HealthToken healthToken
				&& tokenClasses.Contains( args.Token.Class )
			)
				// Downgrade the existing tokens health
				// AND change what we are removing to be the downgraded token
				// tokens being destroyed may reduce the count also.
				(args.Token, args.Count) = await ctx.Tokens.AdjustHealthOf( healthToken, -deltaHealth, args.Count, args.ActionId );
		} );
		ctx.Tokens.Adjust( removingToken, 1 );

		// Add handler to restore ALL at end of round.
		ctx.GameState.TimePasses_ThisRound.Push( async gs => {
			// Ensure the dahan are healed before we restore their health.
			ctx.GameState.Healer.HealSpace( ctx.Tokens );
			await ctx.Tokens.AdjustHealthOfAll( -deltaHealth, ctx.ActionCtx, tokenClasses );
		} );
	}

}