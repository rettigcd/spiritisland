﻿using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	public class DahanReclaimFishingGrounds : IFearOptions {
		
		public const string Name = "Dahan Reclaim Fishing Grounds";
		[FearLevel(1, "Each player chooses a different Coastal land with Dahan. In each: 1 Damage per Dahan." )]
		public Task Level1( FearCtx ctx ) {

			return SpiritsActOnDifferentCostalLands( ctx, 
				spaceCtx => spaceCtx.DamageInvaders(1)
			);

		}

		[FearLevel(2, "Each player chooses a different Coastal land. In each: Gather up to 1 Dahan. 1 Damage per Dahan." )]
		public Task Level2( FearCtx ctx ) { 
			return SpiritsActOnDifferentCostalLands( ctx, 
				async spaceCtx => {
					await spaceCtx.GatherUpToNDahan( 1 );
					await spaceCtx.DamageInvaders(1);
				}
			);
		}

		[FearLevel(3, "Each player chooses a different Coastal land. In each: Gather up to 1 Dahan. 2 Damage per Dahan." )]
		public Task Level3( FearCtx ctx ) {
			return SpiritsActOnDifferentCostalLands( ctx, 
				async spaceCtx => {
					await spaceCtx.GatherUpToNDahan( 1 );
					await spaceCtx.DamageInvaders(2);
				}
			);
		}

		static async Task SpiritsActOnDifferentCostalLands( FearCtx ctx, Func<TargetSpaceCtx, Task> act ) {
			var options = ctx.GameState.Island.AllSpaces
				.Where(s=>s.IsCoastal && ctx.GameState.Tokens[s].Dahan.Any)
				.ToList();

			foreach( var spirit in ctx.Spirits ) {
				if(options.Count == 0) break;
				var spaceCtx = await spirit.SelectSpace("1 damage per Dahan", options);
				if( spaceCtx != null) {
					await act(spaceCtx);
					options.Remove(spaceCtx.Space);
				}
			}
		}

	}


}
