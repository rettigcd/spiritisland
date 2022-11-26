namespace SpiritIsland;

public static class HealthAdjustmentHelper_Extension {

	public static async Task AdjustTokensHealthForRound( this TargetSpaceCtx ctx, int deltaHealth, params HealthTokenClass[] tokenClasses ) {

		// Adjust in land
		await ctx.Tokens.AdjustHealthOfAll( deltaHealth, ctx.CurrentActionId, tokenClasses ); // ActionID needed if token is destoryed.

		bool Matches(Space space, TokenClass tokenClass) => space == ctx.Space && tokenClasses.Contains(tokenClass);

		// Add (covers simple-add AND move-in)
		ctx.GameState.Tokens.AddingToken.ForRound.Add( args => {
			if( args.Token is HealthToken healthToken && Matches( args.Space,args.Token.Class ) )
				args.Token = healthToken.AddHealth( deltaHealth );
		} );

		// Remove (covers simple-remove AND move-out)
		ctx.GameState.Tokens.RemovingToken.ForRound.Add( async args => {
			if( args.Token is HealthToken healthToken && Matches( args.Space, args.Token.Class ) )
				// Downgrade the existing tokens health
				// AND change what we are removing to be the downgraded token
				// tokens being destroyed may reduce the count also.
				(args.Token,args.Count) = await ctx.Tokens.AdjustHealthOf( healthToken, -deltaHealth, args.Count, args.ActionId );
		} );

		// Add handler to restore ALL at end of round.
		ctx.GameState.TimePasses_ThisRound.Push( async gs => {
			// Ensure the dahan are healed before we restore their health.
			ctx.GameState.Healer.HealSpace( ctx.Tokens );
			await ctx.Tokens.AdjustHealthOfAll( -deltaHealth, gs.StartAction(), tokenClasses ); // this action id is only used if tokens get destroyed
		} );
	}

}