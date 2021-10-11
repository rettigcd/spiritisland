﻿using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	class DisorientingLandscape {

		[MinorCard( "Disorienting Landscape", 1, Element.Moon, Element.Air, Element.Plant )]
		[Fast]
		[FromSacredSite( 2 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			await ctx.Push(1, Invader.Explorer);

			if(ctx.Space.Terrain.IsOneOf(Terrain.Mountain,Terrain.Jungle))
				ctx.Tokens.Wilds().Count++;
		}

	}


}
