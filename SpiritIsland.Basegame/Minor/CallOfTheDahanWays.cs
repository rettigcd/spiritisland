﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class CallOfTheDahanWays {

		[MinorCard("Call of the Dahan Ways",1,Element.Moon,Element.Water,Element.Animal), Slow, FromPresence(1,Target.Dahan)]
		static public async Task Act(TargetSpaceCtx ctx){

			// if you have 2 moon, you may instead replace 1 town with 1 dahan
			if(ctx.Tokens.Has(Invader.Town) && await ctx.YouHave("2 moon")) {
				ctx.RemoveInvader( Invader.Town ); // !!! ??? reports event?
			} else if(ctx.Tokens.Has(Invader.Explorer)) {
				// replace 1 explorer with 1 dahan
				ctx.Adjust( Invader.Explorer[1], -1 ); //!!! report event
			}
			await ctx.Dahan.Add( 1,AddReason.AsReplacement );
		}

	}

}
