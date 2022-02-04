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
		return tokens.Invaders().OfType<StrifedInvader>().Sum( si => si.FullHealth * tokens[si] );
	}

	public static int DamageFrom1StrifedInvaders( TokenCountDictionary tokens ) {
		var strifedInvaderWithMostDamage = tokens.Invaders().OfType<StrifedInvader>()
			.OrderByDescending(x=>x.FullHealth)
			.FirstOrDefault();
		return strifedInvaderWithMostDamage != null ? strifedInvaderWithMostDamage.FullHealth : 0;
	}


	static public Task DamageUnStriffed( TargetSpaceCtx invaderSpaceCtx, int damageFromStrifedInvaders ) {
		// !!! this isn't 100% correct, the damage will start applying to unstrifed, but will then spill over onto strifed
		// ! Fix by passing in a predicate
		return invaderSpaceCtx.DamageInvaders( damageFromStrifedInvaders );
	}

	public static void StrifedInvadersLoseHealthPerStrife( FearCtx ctx ) {
		// Fear effect - Strifed invades lose 1 health per strife down to minimun of 1

		foreach(var space in ctx.GameState.Island.AllSpaces) {
			var tokens = ctx.InvadersOn( space ).Tokens;
			var strifedInvaders = tokens.Invaders()
				.OfType<StrifedInvader>()
				.Where( x => x.Health > 1 )
				.OrderBy( x => x.Health ); // get the lowest ones first so we can reduce without them cascading
			foreach(StrifedInvader strifedInvader in strifedInvaders) {
				var newInvader = strifedInvader.ResultingDamagedInvader( strifedInvader.StrifeCount );
				if(newInvader.Health > 0) {
					tokens.Adjust(newInvader, tokens[strifedInvader]);
					tokens.Init(strifedInvader, 0);
				}
			}
		}
	}

}