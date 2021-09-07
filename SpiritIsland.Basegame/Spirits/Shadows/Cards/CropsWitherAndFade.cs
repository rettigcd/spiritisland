﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class CropsWitherAndFade {

		[SpiritCard("Crops Wither and Fade",1,Speed.Slow,Element.Moon,Element.Fire,Element.Plant)]
		[FromPresence(0)]
		static public Task ActAsync( TargetSpaceCtx ctx ){

			// 2 fear
			ctx.AddFear( 2 );

			return ctx.SelectActionOption(
				new ActionOption("replace town with explorer", () => Replace1(ctx,Invader.Town,Invader.Explorer)),
				new ActionOption("reaplce city with town", () => Replace1(ctx,Invader.City,Invader.Town))
			);

		}

		static async Task Replace1( TargetSpaceCtx ctx, TokenGroup group, TokenGroup newToken ) {
			var origTokens = ctx.Tokens.OfType(group);
			var oldToken = await ctx.Self.Action.Decide( new SelectInvaderToDowngrade( ctx.Target, origTokens, Present.IfMoreThan1 ) );
			if(oldToken != null) return;

			ctx.Tokens.Adjust( oldToken, -1 );
			ctx.Tokens.Adjust( newToken.Default, 1 );
		}

	}

}
