namespace SpiritIsland;

public static class StrifedRavage {

	// Theological Strife(3) - Each Invader with Strife deals Damage to other Invaders in its land.
	// Instruments of their own ruin
	// Discord
	static public SpaceAction StrifedInvadersDealsDamageToOtherInvaders => new SpaceAction(
		"Strifed invaders deal damage to other invaders.",
		async ctx => {

			// Capture Strifed Counts
			var strifedCounts = ctx.Space.InvaderTokens()
				.Where( x => 0 < x.StrifeCount )
				.ToDictionary( x=>x, x=>ctx.Space[x] );

			var invaderBinding = ctx.Invaders;
			foreach(var p in strifedCounts)
				await ctx.StrifedDamageOtherInvaders( 
					p.Value * p.Key.Attack, // total damage from this type.
					p.Key, // the source of the damage
					p.Value==1 // exclude source if there is only 1 - it can't damage itself.
				);
		}
	);

	#region Strife caused Damage to Self

	public static IActOn<BoardCtx> StrifedInvadersTakeDamagePerStrife 
		=> new BaseCmd<BoardCtx>( "each invader takes 1 damage per strife it has", StrifedInvadersTakeDamagePerStrifeImp );

	static async Task StrifedInvadersTakeDamagePerStrifeImp( BoardCtx boardCtx ) {
		foreach(var space in boardCtx.Board.Spaces.ScopeTokens())
			await EachInvaderTakesDamageByStrifeCount( space );
	}

	static async Task EachInvaderTakesDamageByStrifeCount( Space space ) {
		var strifedInvaders = space.InvaderTokens()
			.Where( x => 0 < x.StrifeCount )
			.OrderBy( x => x.RemainingHealth )
			.ToArray(); // get the lowest ones first so we can reduce without them cascading

		// ! Not doing Badland damage here since I don't want to an a UI/await

		foreach(HumanToken strifedInvader in strifedInvaders)
			await DamageInvaderHealthByItsOwnStrife( space, strifedInvader );
	}

	static async Task DamageInvaderHealthByItsOwnStrife( Space space, HumanToken invader ) {
		await space.AllHumans( invader ).AdjustAsync( x=> x.AddDamage( x.StrifeCount ) );
	}

	#endregion

}
