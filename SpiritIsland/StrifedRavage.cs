namespace SpiritIsland;

public static class StrifedRavage {

	static public SpaceAction Cmd => new SpaceAction(
		"Strifed invaders deal damage to other invaders.", 
		StrifedInvadersDamageUnstrifed
	);

	public static async Task StrifedInvadersDamageUnstrifed( TargetSpaceCtx ctx ) {
		// Each invader with strife deals damage to other invaders in target land
		int damageFromStrifedInvaders = DamageFromStrifedInvaders( ctx.Tokens );
		await DamageUnStriffed( ctx, damageFromStrifedInvaders );
	}

	public static int DamageFromStrifedInvaders( TokenCountDictionary tokens ) {
		return tokens.Invaders().OfType<HealthToken>().Where(x=>x.StrifeCount>0).Sum( si => si.FullHealth * tokens[si] );
	}

	public static int DamageFrom1StrifedInvaders( TokenCountDictionary tokens ) {
		var strifedInvaderWithMostDamage = tokens.Invaders().OfType<HealthToken>()
			.OrderByDescending(x=>x.FullHealth)
			.FirstOrDefault();
		return strifedInvaderWithMostDamage != null ? strifedInvaderWithMostDamage.FullHealth : 0;
	}


	static public Task DamageUnStriffed( TargetSpaceCtx invaderSpaceCtx, int damageFromStrifedInvaders ) {
		// !!! this isn't 100% correct, the damage will start applying to unstrifed, but will then spill over onto strifed
		// ! Fix by passing in a predicate
		return invaderSpaceCtx.DamageInvaders( damageFromStrifedInvaders );
	}

	#region Strife reduces Health

	public static async Task InvadersReduceHealthByStrifeCount( GameState gameState, Guid actionId ) {
		// add penalty
		++HealthToken.HealthPenaltyPerStrife;
		// remove penalty
		gameState.TimePasses_ThisRound.Push( x => { --HealthToken.HealthPenaltyPerStrife; return Task.CompletedTask; } );

		// Check if anything is destroyed
		foreach(var space in gameState.Island.AllSpaces) {
			var tokens = gameState.Tokens[space];
			foreach( var token in tokens.Invaders() )
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
		var strifedInvaders = tokens.Invaders()
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