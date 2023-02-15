namespace SpiritIsland;

public static class HealthAdjustmentHelper_Extension {

	public static async Task AdjustTokensHealthForRound( this TargetSpaceCtx ctx, int deltaHealth, params HumanTokenClass[] tokenClasses ) {
		// Doing this on TargetSpaceCtx because adjusting health could destroy a town.

		await ctx.Tokens.AdjustHealthOfAll( deltaHealth, tokenClasses );
		ctx.Tokens.Adjust( new HealthAdjustmentToken( deltaHealth, tokenClasses ), 1 );
	}

	public class HealthAdjustmentToken : BaseModEntity
		, IHandleAddingToken
		, IHandleRemovingToken
		, ISpaceEntityWithEndOfRoundCleanup
	{

		public HealthAdjustmentToken( int deltaHealth, params HumanTokenClass[] tokenClasses ) { 
			_deltaHealth = deltaHealth;
			_tokenClasses = tokenClasses;
		}

		// Add (covers simple-add AND move-in)
		public void ModifyAdding( AddingTokenArgs args ){
			if(args.Token is HumanToken healthToken && _tokenClasses.Contains( args.Token.Class ))
				args.Token = healthToken.AddHealth( _deltaHealth );
		}

		// Remove (covers simple-remove AND move-out)
		public async Task ModifyRemoving( RemovingTokenArgs args ) {
			if(args.Mode == RemoveMode.Test) return;
			if(args.Token is HumanToken healthToken && _tokenClasses.Contains( args.Token.Class ))
				// Downgrade the existing tokens health
				// AND change what we are removing to be the downgraded token
				// tokens being destroyed may reduce the count also.
				(args.Token, args.Count) = await args.Space.AdjustHealthOf( healthToken, -_deltaHealth, args.Count );
		}

		// Cleanup
		public void EndOfRoundCleanup( SpaceState tokens ) {
			GameState.Current.Healer.HealSpace( tokens );
			tokens.AdjustHealthOfAll( -_deltaHealth, _tokenClasses ).Wait(); // This should be ok because tokens are healed.
			tokens.Init(this,0);
		}

		readonly HumanTokenClass[] _tokenClasses;
		readonly int _deltaHealth;

	}

}