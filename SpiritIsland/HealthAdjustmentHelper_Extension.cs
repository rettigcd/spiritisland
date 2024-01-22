namespace SpiritIsland;

public static class HealthAdjustmentHelper_Extension {

	/// <summary>
	/// Initiates a health bonus in a given SpaceState until the end of the round.
	/// </summary>
	static public async Task AdjustTokensHealthForRound( this TargetSpaceCtx ctx, int deltaHealth, params HumanTokenClass[] tokenClasses ) {
		// Doing this on TargetSpaceCtx because adjusting health could destroy a town.

		await ctx.Tokens.AdjustHealthOfAll( deltaHealth, tokenClasses );
		ctx.Tokens.Adjust( new HealthAdjustmentToken( deltaHealth, tokenClasses ), 1 );
	}

	static public async Task AdjustHealthOfAll( this SpaceState spaceState, int delta, params HumanTokenClass[] tokenClasses ) {
		if(delta == 0) return;
		foreach(var tokenClass in tokenClasses) {
			var tokens = spaceState.HumanOfTag( tokenClass );
			HumanToken[] orderedTokens = delta < 0
				? [.. tokens.OrderBy( x => x.FullHealth )]
				: [.. tokens.OrderByDescending( x => x.FullHealth )];
			foreach(var token in orderedTokens)
				await spaceState.AllHumans( token ).AdjustHealthAsync( delta );
		}
	}

	/// <summary>
	/// Modifies (specified) Human Tokens to gain health bonus when in a particular land 
	/// and lose it when they leave or at the end of the round.
	/// </summary>
	public class HealthAdjustmentToken : BaseModEntity
		, IModifyAddingToken
		, IModifyRemovingTokenAsync
		, ISpaceEntityWithEndOfRoundCleanup
	{

		public HealthAdjustmentToken( int deltaHealth, params HumanTokenClass[] tokenClasses ) { 
			_bonusHealth = deltaHealth;
			_tokenClasses = tokenClasses;
		}

		// Add (covers simple-add AND move-in)
		public void ModifyAdding( AddingTokenArgs args ){
			if(args.Token is HumanToken healthToken && _tokenClasses.Contains( args.Token.Class ))
				args.Token = healthToken.AddHealth( _bonusHealth );
		}

		// Remove (covers simple-remove AND move-out)
		public async Task ModifyRemovingAsync( RemovingTokenArgs args ) {
			if(args.Token is HumanToken healthToken && _tokenClasses.Contains( args.Token.Class )) {
				// Downgrade the existing tokens health
				// AND change what we are removing to be the downgraded token
				// tokens being destroyed may reduce the count also.
				var result = await args.From.Humans( args.Count, healthToken )
					.AdjustHealthAsync( -_bonusHealth );
				args.Token = result.NewToken;
				args.Count = result.Count;
			}
		}

		// Cleanup
		public void EndOfRoundCleanup( SpaceState tokens ) {
			GameState.Current.Healer.HealSpace( tokens );
			tokens.AdjustHealthOfAll( -_bonusHealth, _tokenClasses ).Wait(); // This should be ok because tokens are healed.
			tokens.Init(this,0);
		}

		readonly HumanTokenClass[] _tokenClasses;
		readonly int _bonusHealth;

	}

}