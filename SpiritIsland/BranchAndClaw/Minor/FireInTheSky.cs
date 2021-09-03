﻿using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class FireInTheSky {

		[MinorCard("Fire in the Sky",1,Speed.Fast,Element.Sun,Element.Fire,Element.Air)]
		[FromSacredSite(1)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {
			ctx.AddFear( 2 );

			await ctx.Self.SelectInvader_ToStrife( ctx.Tokens );

		}

	}

}
