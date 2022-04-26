namespace SpiritIsland;

public static class DahanHelper {

	public static async Task BoostDahanHealthForRound( TargetSpaceCtx ctx, int boost ) {
		// Boost ALL Dahan in land
		await ctx.Dahan.AdjustHealthOfAll( boost );

		// Move(,) triggers 'Add' and 'Remove' events also

		// This covers simple-add AND move-in
		ctx.GameState.Tokens.TokenAdded.ForRound.Add( async args => {
			if(args.Token.Class == TokenType.Dahan && args.Space == ctx.Space)
				await ctx.Dahan.AdjustHealthOf( (HealthToken)args.Token, boost, args.Count );
		} );


		// Move out
		ctx.GameState.Tokens.TokenMoved.ForRound.Add( async args => {
			// if removing dahan from this space
			if(args.Token.Class == TokenType.Dahan && args.RemovedFrom == ctx.Space)
				// In the destination space
				await ctx.Target(args.AddedTo)
					// reduce the health by boost amount
					.Dahan.AdjustHealthOf( (HealthToken)args.Token, -boost, args.Count );
		} );

		// Add handler to restore ALL at end of round.
		ctx.GameState.TimePasses_ThisRound.Push( async gs => {
			// Ensure the dahan are healed before we restore their health.
			ctx.GameState.Healer.HealSpace( ctx.Tokens );
			await ctx.Dahan.AdjustHealthOfAll( -boost );
		} );
	}

}