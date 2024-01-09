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
				.Where( x => 0 < x.StrifeCount )
				.ToDictionary( x=>x, x=>ctx.Tokens[x] );

			var invaderBinding = ctx.Invaders;
			foreach(var p in strifedCounts)
				await ctx.StrifedDamageOtherInvaders( 
					p.Value * p.Key.Attack, // total damage from this type.
					p.Key, // the source of the damage
					p.Value==1 // exclude source if there is only 1 - it can't damage itself.
				);
		}
	);

	#region Strife reduces Health

	public static async Task InvadersReduceHealthByStrifeCount( GameState gameState ) {
		// add penalty
		++gameState.HealthPenaltyPerStrife;
		// remove penalty
		gameState.AddTimePassesAction( TimePassesAction.Once( gs=>--gs.HealthPenaltyPerStrife ) );

		// Check if anything is destroyed
		foreach(var space in gameState.Spaces)
			foreach( var token in space.InvaderTokens() )
				if(token.IsDestroyed)
					await space.Destroy( token, space[token] );
	}

	#endregion

	#region Strife caused Damage to Self

	public static IActOn<BoardCtx> StrifedInvadersTakeDamagePerStrife 
		=> new BaseCmd<BoardCtx>( "each invader takes 1 damage per strife it has", StrifedInvadersTakeDamagePerStrifeImp );

	static async Task StrifedInvadersTakeDamagePerStrifeImp( BoardCtx boardCtx ) {
		foreach(var space in boardCtx.Board.Spaces.Tokens())
			await EachInvaderTakesDamageByStrifeCount( space );
	}

	static async Task EachInvaderTakesDamageByStrifeCount( SpaceState tokens ) {
		var strifedInvaders = tokens.InvaderTokens()
			.Where( x => 0 < x.StrifeCount )
			.OrderBy( x => x.RemainingHealth )
			.ToArray(); // get the lowest ones first so we can reduce without them cascading

		// ! Not doing Badland damage here since I don't want to an a UI/await

		foreach(HumanToken strifedInvader in strifedInvaders)
			await DamageInvaderHealthByItsOwnStrife( tokens, strifedInvader );
	}

	static async Task DamageInvaderHealthByItsOwnStrife( SpaceState tokens, HumanToken originalInvader ) {
		var newInvader = originalInvader.AddDamage( originalInvader.StrifeCount );
		if(newInvader == originalInvader) return;

		if(newInvader.IsDestroyed)
			await tokens.Destroy( originalInvader, tokens[originalInvader] );
		else {
			tokens.AdjustPropsForAll( originalInvader ).To( newInvader );
		}
	}

	#endregion

}