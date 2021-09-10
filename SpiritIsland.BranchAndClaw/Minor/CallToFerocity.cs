﻿using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class CallToFerocity {

		[MinorCard( "Call to Ferocity", 0, Speed.Slow, Element.Sun, Element.Fire, Element.Earth )]
		[FromPresence( 1, Target.Invaders )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			return ctx.SelectActionOption(
				new ActionOption( "Gather up to 3 dahan", () => ctx.GatherUpToNDahan( 3 ) ),
				new ActionOption( "1 fear and push 1 explorer and 1 town", ()=>Opt2(ctx) )
			);
		}

		static async Task Opt2( TargetSpaceCtx ctx ) {
			ctx.AddFear( 1 );
			await ctx.PushN( ctx.Space, 1, Invader.Explorer );
			await ctx.PushN( ctx.Space, 1, Invader.Town );
		}
	}

}
