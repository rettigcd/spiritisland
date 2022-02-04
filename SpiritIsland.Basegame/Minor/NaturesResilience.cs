﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class NaturesResilience {

		public const string Name = "Nature's Resilience";

		[MinorCard(NaturesResilience.Name,1,Element.Earth,Element.Plant,Element.Animal)]
		[Fast]
		[FromSacredSite(1)]
		static public async Task Act(TargetSpaceCtx ctx){

			await ctx.SelectActionOption(
				new SpaceAction("Defend 6", ctx=>ctx.Defend(6)),
				new SpaceAction("Remove 1 blight", ctx=>ctx.RemoveBlight() )
					// !! This condition changes state for Shifting Memories, only use inside SelectActionOption
					// Also, make sure there is actually blight there before asking spirit (Shifting Memory) if they want to commit 2 water
					.FilterOption( ctx.HasBlight && await ctx.YouHave("2 water") )
			);

		}

	}
}
