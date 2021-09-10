﻿using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {
	class DisorientingLandscape {

		[MinorCard( "Disorienting Landscape", 1, Speed.Fast, Element.Moon, Element.Air, Element.Plant )]
		[FromSacredSite( 2 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			await ctx.Push(1, Invader.Explorer);

			if(ctx.Space.Terrain.IsIn(Terrain.Mountain,Terrain.Jungle))
				ctx.Tokens.Wilds().Count++;
		}

	}


}
