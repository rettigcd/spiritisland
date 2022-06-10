namespace SpiritIsland;

public static class StrifedRavage {

	// Theological Strife(3) - Each Invader with Strife deals Damage to other Invaders in its land.
	// Instruments of their own ruin
	// Discord
	static public SpaceAction StrifedInvadersDealsDamageToOtherInvaders => new SpaceAction(
		"Strifed invaders deal damage to other invaders.",
		async ctx => {

			// Capture Strifed Counts
			var strifedCounts = ctx.Tokens.InvaderTokens()
				.Where( x => x.StrifeCount > 0 )
				.ToDictionary( x=>x, x=>ctx.Tokens[x] );

			var invaderBinding = ctx.Invaders;
			foreach(var p in strifedCounts)
				await ctx.StrifedDamageOtherInvaders( 
					p.Value * invaderBinding.Tokens.AttackDamageFrom1( p.Key ), // total damage from this type.
					p.Key, // the source of the damage
					p.Value==1 // exclude source if there is only 1 - it can't damage itself.
				);
		}
	);

	#region Strife reduces Health

	public static async Task InvadersReduceHealthByStrifeCount( GameState gameState, Guid actionId ) {
		// add penalty
		++HealthToken.HealthPenaltyPerStrife;
		// remove penalty
		gameState.TimePasses_ThisRound.Push( x => { --HealthToken.HealthPenaltyPerStrife; return Task.CompletedTask; } );

		// Check if anything is destroyed
		foreach(var space in gameState.Island.AllSpaces) {
			var tokens = gameState.Tokens[space];
			foreach( var token in tokens.InvaderTokens() )
				if(token.IsDestroyed)
					await tokens.Destroy( token, tokens[token], actionId );
		}
	}

	#endregion

	#region Strife caused Damage to Self

	public static async Task StrifedInvadersTakeDamagePerStrife( FearCtx ctx ) {
		Guid actionId = Guid.NewGuid();
		foreach(var space in ctx.GameState.Island.AllSpaces)
			await EachInvaderTakesDamageByStrifeCount( ctx.GameState.Tokens[space], actionId );
	}

	static async Task EachInvaderTakesDamageByStrifeCount( TokenCountDictionary tokens, Guid actionId ) {
		var strifedInvaders = tokens.InvaderTokens()
			.Where( x => 0 < x.StrifeCount )
			.OrderBy( x => x.RemainingHealth )
			.ToArray(); // get the lowest ones first so we can reduce without them cascading

		// !!! ??? Do badlands cause damage here?

		foreach(HealthToken strifedInvader in strifedInvaders)
			await DamageInvaderHealthByItsOwnStrife( tokens, strifedInvader, actionId );
	}

	static async Task DamageInvaderHealthByItsOwnStrife( TokenCountDictionary tokens, HealthToken originalInvader, Guid actionId ) {
		var newInvader = originalInvader.AddDamage( originalInvader.StrifeCount );
		if(newInvader == originalInvader) return;

		if(newInvader.IsDestroyed)
			await tokens.Destroy( originalInvader, tokens[originalInvader], actionId );
		else {
			tokens.Adjust( newInvader, tokens[originalInvader] );
			tokens.Init( originalInvader, 0 );
		}
	}

	#endregion

}