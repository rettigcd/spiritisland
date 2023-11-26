namespace SpiritIsland.BranchAndClaw;

public class VolcanicEruption {

	[MajorCard("Volcanic Eruption", 8, Element.Fire, Element.Earth),Slow,FromPresence(Target.Mountain,1)]
	[Instructions( "6 Fear. 20 Damage. Destroy all Dahan and Beasts. Add 1 Blight. -If you have- 4 Fire, 3 Earth: Destroy all Invaders. Add 1 Wilds. In each adjacent land: 10 Damage. Destroy all Dahan and Beasts. If there are no Blight, add 1 Blight." ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync(TargetSpaceCtx ctx) {
		// 6 fear
		ctx.AddFear( 6 );

		// 20 damage.
		await ctx.DamageInvaders( 20 );

		// Destroy all dahan and beast.
		await DestroyDahanAndBeasts( ctx );

		// Add 1 blight
		await ctx.AddBlight(1);

		// if you have 4 fire, 3 earth:
		if(await ctx.YouHave( "4 fire,3 earth" )) {
			// Destroy all invaders.
			await ctx.Invaders.DestroyAll( Human.Invader );
			// Add 1 wilds.
			await ctx.Wilds.AddAsync(1);
			// In  each adjacent land:
			foreach(var adj in ctx.Adjacent.Select( ctx.Target ))
				await EffectAdjacentLand( adj );
		}

	}

	static async Task EffectAdjacentLand( TargetSpaceCtx adj ) {
		// 10 damage,
		await adj.DamageInvaders( 10 );
		// destroy all dahan and beast.
		await DestroyDahanAndBeasts( adj );
		// IF there are no blight, add 1 blight
		if(adj.Blight.Count == 0)
			await adj.AddBlight(1);
	}

	static async Task DestroyDahanAndBeasts( TargetSpaceCtx ctx ) {
		await ctx.Dahan.DestroyAll();
		await ctx.Beasts.Destroy( ctx.Beasts.Count );
	}

}