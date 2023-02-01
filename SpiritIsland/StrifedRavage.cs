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

	public static async Task InvadersReduceHealthByStrifeCount( GameCtx ctx ) {
		// add penalty
		++ctx.GameState.HealthPenaltyPerStrife;
		// remove penalty
		ctx.GameState.TimePasses_ThisRound.Push( x => { --ctx.GameState.HealthPenaltyPerStrife; return Task.CompletedTask; } );

		// Check if anything is destroyed
		foreach(var space in ctx.GameState.AllActiveSpaces)
			foreach( var token in space.InvaderTokens() )
				if(token.IsDestroyed)
					await space.Bind( ctx.ActionScope ).Destroy( token, space[token] );
	}

	#endregion

	#region Strife caused Damage to Self

	public static async Task StrifedInvadersTakeDamagePerStrife( GameCtx gameCtx ) {
		foreach(var space in gameCtx.GameState.AllActiveSpaces)
			await EachInvaderTakesDamageByStrifeCount( space, gameCtx.ActionScope );
	}

	static async Task EachInvaderTakesDamageByStrifeCount( SpaceState tokens, UnitOfWork actionScope ) {
		var strifedInvaders = tokens.InvaderTokens()
			.Where( x => 0 < x.StrifeCount )
			.OrderBy( x => x.RemainingHealth )
			.ToArray(); // get the lowest ones first so we can reduce without them cascading

		// !!! ??? Do badlands cause damage here?

		foreach(HumanToken strifedInvader in strifedInvaders)
			await DamageInvaderHealthByItsOwnStrife( tokens, strifedInvader, actionScope );
	}

	static async Task DamageInvaderHealthByItsOwnStrife( SpaceState tokens, HumanToken originalInvader, UnitOfWork actionScope ) {
		var newInvader = originalInvader.AddDamage( originalInvader.StrifeCount );
		if(newInvader == originalInvader) return;

		if(newInvader.IsDestroyed)
			await tokens.Bind( actionScope ).Destroy( originalInvader, tokens[originalInvader] );
		else {
			tokens.Adjust( newInvader, tokens[originalInvader] );
			tokens.Init( originalInvader, 0 );
		}
	}

	#endregion

}