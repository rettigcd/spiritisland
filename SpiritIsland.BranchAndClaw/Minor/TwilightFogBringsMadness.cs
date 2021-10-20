﻿using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class TwilightFogBringsMadness {

		[MinorCard( "Twilight Fog Brings Madness", 0, Element.Sun, Element.Moon, Element.Air, Element.Water )]
		[Slow]
		[FromPresence( 1 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			// Add 1 strife
			await ctx.AddStrife();

			// Push 1 dahan
			await ctx.PushDahan( 1 );

			// Each remaining Dahan take 1 damage
			await RemainingDahanTake1Damage( ctx );

		}

		static async Task RemainingDahanTake1Damage( TargetSpaceCtx ctx ) {

			// This is applying 1 damage to each dahan. (and reports dahan destroyed)

			// !!! There should be a common Destroy-Dahan that could be invoked (I think)
			int dahanDestroyed = ctx.Dahan[1];
			ctx.Dahan[1] = ctx.Dahan[2];
			ctx.Dahan[2] = 0;
			await ctx.GameState.Tokens.TokenDestroyed.InvokeAsync( ctx.GameState, new TokenDestroyedArgs {
				Token = TokenType.Dahan,
				space = ctx.Space,
				count = dahanDestroyed,
				Source = Cause.Power
			} );
		}
	}

}
