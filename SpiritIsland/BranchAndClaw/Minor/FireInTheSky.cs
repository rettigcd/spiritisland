using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class FireInTheSky {

		[MinorCard("Fire in the Sky",1,Speed.Fast,Element.Sun,Element.Fire,Element.Air)]
		[FromSacredSite(1)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {
			ctx.AddFear(2);
			var d = new InvadersOnSpaceDecision( "ff", ctx.Target, ctx.Tokens.Invaders().ToArray(), Present.Always );

			var invader = await ctx.Self.Action.Choose(d);
			if(invader != null)
				ctx.Tokens.AddStrifeTo(invader);

		}

	}

}
