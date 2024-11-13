namespace SpiritIsland;

/// <summary>
/// Modifies (specified) Human Tokens to gain health bonus when in a particular land 
/// and lose it when they leave or at the end of the round.
/// </summary>
public class AdjustHealth(int _deltaHealth, params HumanTokenClass[] _tokenClasses)
	: IModifyAddingToken
	, IModifyRemovingTokenAsync
	, ICleanupSpaceWhenTimePasses {

	public async Task InitOn(Space space) {
		await AdjustHealth.AdjustHealthOfAll(space, _deltaHealth, _tokenClasses);
		space.Adjust(this, 1);
	}

	// Add (covers simple-add AND move-in)
	public void ModifyAdding(AddingTokenArgs args) {
		if( args.Token is HumanToken healthToken && _tokenClasses.Contains(args.Token.Class) )
			args.Token = healthToken.AddHealth(_deltaHealth);
	}

	// Remove (covers simple-remove AND move-out)
	public async Task ModifyRemovingAsync(RemovingTokenArgs args) {
		if( args.Token is HumanToken healthToken && _tokenClasses.Contains(args.Token.Class) ) {
			switch( args.Reason ) {
				case RemoveReason.Abducted:
				case RemoveReason.MovedFrom:
					// Downgrade the existing tokens health
					// AND change what we are removing to be the downgraded token
					// tokens being destroyed may reduce the count also.
					var result = await args.From.Humans(args.Count, healthToken)
						.AdjustHealthAsync(-_deltaHealth);
					args.Token = result.NewToken;
					args.Count = result.Count;
					if( ((HumanToken)args.Token).IsDestroyed )
						args.Count = 0;
					break;

				case RemoveReason.Replaced:
				case RemoveReason.Removed:
				case RemoveReason.Destroyed:
					// no change
					break;

				default:
				case RemoveReason.UsedUp:
				case RemoveReason.TakingFromCard: // should never be called
					throw new Exception($"unexpected remove reason {args.Reason}");
			}
		}
	}

	// Cleanup
	public void EndOfRoundCleanup(Space space) {
		GameState.Current.Healer.HealSpace(space);
		AdjustHealthOfAll(space, -_deltaHealth, _tokenClasses).Wait(); // This should be ok because tokens are healed.
		space.Init(this, 0);
	}

	static public async Task AdjustHealthOfAll(Space space, int delta, params HumanTokenClass[] tokenClasses) {
		if( delta == 0 ) return;
		foreach( var tokenClass in tokenClasses ) {
			var tokens = space.HumanOfTag(tokenClass);
			HumanToken[] orderedTokens = delta < 0
				? [.. tokens.OrderBy(x => x.FullHealth)]
				: [.. tokens.OrderByDescending(x => x.FullHealth)];
			foreach( var token in orderedTokens )
				await space.AllHumans(token).AdjustHealthAsync(delta);
		}
	}

}

public static class HealthAdjustmentHelper_Extension {

	/// <summary>
	/// Initiates a health bonus in a given Space until the end of the round.
	/// </summary>
	static public async Task AdjustTokensHealthForRound( this Space space, int deltaHealth, params HumanTokenClass[] tokenClasses ) {
		await new AdjustHealth(deltaHealth, tokenClasses).InitOn(space);
	}

}