﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class RiversBounty {

		public const string Name = "River's Bounty";
		[SpiritCard(RiversBounty.Name, 0, Speed.Slow,Element.Sun,Element.Water,Element.Animal)]
		[FromPresence(0)]
		static public async Task ActionAsync(TargetSpaceCtx ctx) {

			// Gather up to 2 Dahan
			await ctx.GatherUpTo( ctx.Space, 2, TokenType.Dahan );

			// If there are now at least 2 dahan, then add 1 dahan and gain 1 energy
			if(2 <= ctx.DahanCount) {
				ctx.AdjustDahan( 1 );
				++ctx.Self.Energy;
			}
		}

	}

}



