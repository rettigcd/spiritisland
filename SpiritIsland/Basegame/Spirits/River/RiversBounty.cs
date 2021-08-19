﻿using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	public class RiversBounty {

		public const string Name = "River's Bounty";
		[SpiritCard(RiversBounty.Name, 0, Speed.Slow,Element.Sun,Element.Water,Element.Animal)]
		[FromPresence(0)]
		static public async Task ActionAsync(TargetSpaceCtx ctx) {
			var (self, gameState) = ctx;
			var target = ctx.Target;

			// Gather up to 2 Dahan
			await ctx.GatherUpToNDahan( target, 2 );

			// If there are now at least 2 dahan, then add 1 dahan and gain 1 energy
			if(gameState.GetDahanOnSpace( target ) >= 2) {
				gameState.AdjustDahan( target, 1 );
				++self.Energy;
			}
		}

	}

}



