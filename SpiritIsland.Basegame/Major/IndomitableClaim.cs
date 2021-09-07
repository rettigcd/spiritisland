﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	public class IndomitableClaim {

		[MajorCard( "Indomitable Claim", 4, Speed.Fast, Element.Sun, Element.Earth )]
		[FromPresence( 1 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			// add 1 presence in target land even if you normally could not due to land type.
			var source = await ctx.Self.SelectTrack();
			await ctx.Self.Presence.PlaceFromBoard(source,ctx.Target,ctx.GameState);
			// Defend 20
			ctx.Defend(20);
			// if you have 2 sun, 3 earth,
			if(ctx.Self.Elements.Contains("2 sun,3 earth" )) {
				// 3 fear
				ctx.AddFear(3);
				// if invaders are present, Invaders skip all actions in target land this turn.
				if(ctx.HasInvaders)
					ctx.GameState.SkipAllInvaderActions( ctx.Target );
			}
		}

	}
}
