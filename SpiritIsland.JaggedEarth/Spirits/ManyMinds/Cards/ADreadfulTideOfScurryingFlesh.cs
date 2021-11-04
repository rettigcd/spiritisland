﻿using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class ADreadfulTideOfScurryingFlesh {

		[SpiritCard("A Dreadful Tide of Scurrying Flesh",0, Element.Moon, Element.Air,Element.Water), Fast, FromSacredSite(1,Target.TwoBeasts)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {
			// Remove up to half (round down) of beast in target land.
			int removable = ctx.Beasts.Count / 2;
			int removed = await ctx.Self.SelectNumber("# of Beasts to Remove for 2 fear & skip one invader action", removable,0);
			ctx.Beasts.Count -= removed;

			// For each beast Removed,
			// 2 fear
			ctx.AddFear( 2*removed );
			// and skip one Invader Action
			for(int i = 0; i < removed; ++i) {
				await ctx.SelectActionOption( "Skip Invader Action",
					new ActionOption("Ravage", () => ctx.GameState.SkipRavage(ctx.Space)),
					new ActionOption("Build", () => ctx.GameState.Skip1Build(ctx.Space)),
					new ActionOption("Explore", () => ctx.GameState.SkipExplore(ctx.Space))
				);
			}
		}
	}


}
