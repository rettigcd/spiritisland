namespace SpiritIsland;

public static class DahanHelper {

	public static async Task BoostDahanHealthForRound( TargetSpaceCtx ctx, int boost ) {
		// Boost ALL Dahan in land
		await ctx.Dahan.AdjustHealthOfAll( boost );

		ctx.GameState.Tokens.TokenMoved.ForRound.Add( async args => {
			// Add handler to Boost dahan added:
			if(args.Token.Class != TokenType.Dahan) return;
			HealthToken ht = (HealthToken)args.Token;
			// ...Boost dahan added
			if(args.AddedTo == ctx.Space)
				await ctx.Dahan.AdjustHealthOf( ht, boost, args.Count );
			// ...restore dahan when moved out
			else if(args.RemovedFrom == ctx.Space)
				await ctx.Dahan.AdjustHealthOf( ht, -boost, args.Count );
		} );

		ctx.GameState.Tokens.TokenAdded.ForRound.Add( async args => {
			if(args.Token.Class != TokenType.Dahan) return;
			HealthToken ht = (HealthToken)args.Token;
			await ctx.Dahan.AdjustHealthOf( ht, boost, args.Count );
		} );

		// Add handler to restore ALL at end of round.
		ctx.GameState.TimePasses_ThisRound.Push( async gs => {
			// Ensure the dahan are healed before we restore their health.
			ctx.GameState.Healer.HealSpace( ctx.Tokens );
			await ctx.Dahan.AdjustHealthOfAll( -boost );
		} );
	}

}