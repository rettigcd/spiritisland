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

	public static async Task InvadersReduceHealthByStrifeCount( GameState gameState, int minimum = 1 ) {
		foreach(var space in gameState.Island.AllSpaces)
			await EachInvaderReduceHealthByStrifeCount( gameState.Tokens[space], minimum );
	}

	static async Task EachInvaderReduceHealthByStrifeCount( TokenCountDictionary tokens, int minimum ) {
		var strifedInvaders = tokens.Invaders()
			.Where( x => 0 < x.StrifeCount )
			.OrderBy( x => x.RemainingHealth )
			.ToArray(); // get the lowest ones first so we can reduce without them cascading

		foreach(HealthToken strifedInvader in strifedInvaders)
			await ReduceInvaderHealthByItsOwnStrife( tokens, strifedInvader, minimum );
	}

	static async Task ReduceInvaderHealthByItsOwnStrife( TokenCountDictionary tokens, HealthToken originalInvader, int minimum ) {
		int newHealth = Math.Min( minimum,originalInvader.FullHealth - originalInvader.StrifeCount);
		var newInvader = new HealthToken( originalInvader.Class, newHealth, originalInvader.Damage, originalInvader.StrifeCount );

		if(newInvader == originalInvader) return;

		if(newInvader.IsDestroyed)
			await tokens.Destroy( originalInvader, tokens[originalInvader] );
		else {
			tokens.Adjust( newInvader, tokens[originalInvader] );
			tokens.Init( originalInvader, 0 );
			// !!! Need something at end of turn to restore health.
		}
	}

	#endregion

	#region Strife caused Damage to Self

	public static async Task StrifedInvadersTakeDamagePerStrife( FearCtx ctx ) {
		foreach(var space in ctx.GameState.Island.AllSpaces)
			await EachInvaderTakesDamageByStrifeCount( ctx.GameState.Tokens[space] );
	}

	static async Task EachInvaderTakesDamageByStrifeCount( TokenCountDictionary tokens ) {
		var strifedInvaders = tokens.Invaders()
			.Where( x => 0 < x.StrifeCount )
			.OrderBy( x => x.RemainingHealth )
			.ToArray(); // get the lowest ones first so we can reduce without them cascading

		// !!! ??? Do badlands cause damage here?

		foreach(HealthToken strifedInvader in strifedInvaders)
			await DamageInvaderHealthByItsOwnStrife( tokens, strifedInvader );
	}

	static async Task DamageInvaderHealthByItsOwnStrife( TokenCountDictionary tokens, HealthToken originalInvader ) {
		var newInvader = originalInvader.AddDamage( originalInvader.StrifeCount );
		if(newInvader == originalInvader) return;

		if(newInvader.IsDestroyed)
			await tokens.Destroy( originalInvader, tokens[originalInvader] );
		else {
			tokens.Adjust( newInvader, tokens[originalInvader] );
			tokens.Init( originalInvader, 0 );
		}
	}

	#endregion

}