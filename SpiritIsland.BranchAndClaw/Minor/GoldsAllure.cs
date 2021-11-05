﻿using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class GoldsAllure {

		[MinorCard("Gold's Allure",0,Element.Fire,Element.Earth,Element.Animal)]
		[Slow]
		[FromPresence(1,Target.Mountain)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {
			await ctx.Gather(1,Invader.Explorer);
			await ctx.Gather(1,Invader.Town);
			await ctx.AddStrife();
		}

	}

}
