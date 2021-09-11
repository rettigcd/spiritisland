﻿using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class VolcanicEruption {
        [MajorCard("Volcanic Eruption", 8, Speed.Slow, Element.Fire, Element.Earth)]
        [FromPresenceIn(1,Terrain.Mountain)]
        static public async Task ActAsync(TargetSpaceCtx ctx) {
			// 6 fear
			ctx.AddFear( 6 );

			// 20 damage.
			await ctx.DamageInvaders( 20 );

			// Destroy all dahan and beast.
			await DestroyDahanAndBeasts( ctx );

			// Add 1 blight
			ctx.AddBlight();

			// if you have 4 fire, 3 earth:
			if(ctx.YouHave( "4 fire,3 earth" )) {
				// Destory all invaders.
				await ctx.Invaders.DestroyAny( int.MaxValue, Invader.City, Invader.Town, Invader.Explorer );
				// Add 1 wilds.
				ctx.Tokens.Wilds().Count++;
				// In  each adjacent land:
				foreach(var adj in ctx.Adjacents.Select( ctx.TargetSpace ))
					await EffectAdjacentLand( adj );
			}

		}

		static async Task EffectAdjacentLand( TargetSpaceCtx adj ) {
			// 10 damage,
			await adj.DamageInvaders( 10 );
			// destory all dahan and beast.
			await DestroyDahanAndBeasts( adj );
			// IF there are no blight, add 1 blight
			if(adj.Tokens.Blight.Count == 0)
				adj.AddBlight();
		}

		static async Task DestroyDahanAndBeasts( TargetSpaceCtx ctx ) {
			await ctx.DestroyDahan( int.MaxValue );
			ctx.Tokens.Beasts().Count = 0;
		}
	}

}
