namespace SpiritIsland;

public static class HealthAdjustmentHelper_Extension {

	public static async Task AdjustTokensHealthForRound( this TargetSpaceCtx ctx, int deltaHealth, params HealthTokenClass[] tokenClasses ) {

		// Adjust in land
		await ctx.Tokens.AdjustHealthOfAll( deltaHealth, ctx.CurrentActionId, tokenClasses ); // ActionID needed if token is destoryed.

		bool Matches(Space space, TokenClass tokenClass) => space == ctx.Space && tokenClasses.Contains(tokenClass);

		// Move in / Add (,) (Move triggers Add also)
		// This covers simple-add AND move-in
		ctx.GameState.Tokens.TokenAdded.ForRound.Add( async args => {
			if(Matches(args.Space,args.Token.Class ) )
				await ctx.Tokens.AdjustHealthOf( (HealthToken)args.Token, deltaHealth, args.Count, args.ActionId );
		} );

		// Move out
		ctx.GameState.Tokens.TokenMoved.ForRound.Add( async args => {
			// if removing dahan from this space
			if(Matches(args.RemovedFrom,args.Token.Class) )
				// In the destination space
				await ctx.Target(args.AddedTo)
					// reduce the health by boost amount
					.Tokens.AdjustHealthOf( (HealthToken)args.Token, -deltaHealth, args.Count, args.ActionId );
		} );

		// Add handler to restore ALL at end of round.
		ctx.GameState.TimePasses_ThisRound.Push( async gs => {
			// Ensure the dahan are healed before we restore their health.
			ctx.GameState.Healer.HealSpace( ctx.Tokens );
			await ctx.Tokens.AdjustHealthOfAll( -deltaHealth, Guid.NewGuid(), tokenClasses ); // this action id is only used if tokens get destroyed
		} );
	}

}