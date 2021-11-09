﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class WrapInWingsOfSunlight {

		[MajorCard( "Wrap in Wings of Sunlight", 3, Element.Sun, Element.Air, Element.Animal ), Fast, FromPresence(0)]
		static public async Task ActAsync(TargetSpaceCtx ctx) {

			// if you have 2 sun, 2 air, 2 animal, // First Gather up to 3 dahan
			if(await ctx.YouHave( "2 sun,2 air,2 animal" ))
				await ctx.GatherUpToNDahan( 3 );

			// move up to 5 dahan from target land to any land.
			Space destination = await ctx.MoveTokens( max:5, TokenType.Dahan, range:100 );

			// defend 5 in that land
			ctx.Target( destination ).Defend( 5 );

		}

	}

}
