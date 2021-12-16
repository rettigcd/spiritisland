﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class OvergrowInANight {

		[SpiritCard( "Overgrow in a Night", 2, Element.Moon, Element.Plant )]
		[Fast]
		[FromPresence( 1 )]
		static public Task ActionAsync( TargetSpaceCtx ctx ) {

			return ctx.SelectActionOption(
				new SpaceAction("Add 1 presence", async ctx => {
					var from = await ctx.Presence.SelectSource();
					await ctx.Self.PlacePresence( from, ctx.Space, ctx.GameState );
				} ),
				new SpaceAction( "3 fear", ctx => ctx.AddFear(3) )
					.Cond( ctx.HasSelfPresence && ctx.Tokens.HasInvaders() ) // if presence and invaders
			);

		}

	}
}
