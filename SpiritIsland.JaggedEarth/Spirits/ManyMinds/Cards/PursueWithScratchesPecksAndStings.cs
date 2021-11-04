﻿using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class PursueWithScratchesPecksAndStings {

		[SpiritCard("Pursue with Scratches, PEcks, and Stings",0), Fast, FromPresence(2, Target.Beast)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {
			// 1 fear
			ctx.AddFear(1);
			// for each beast past the first, Push 1 explorer / town.
			int pushCount = ctx.Beasts.Count-1;
			if(pushCount > 0)
				await ctx.Pusher.AddGroup(pushCount,Invader.Explorer,Invader.Town).MoveUpToN();
		}

	}

}
