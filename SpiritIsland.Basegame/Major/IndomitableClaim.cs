﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	public class IndomitableClaim {

		[MajorCard( "Indomitable Claim", 4, Speed.Fast, Element.Sun, Element.Earth )]
		[FromPresence( 1 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			// add 1 presence in target land even if you normally could not due to land type.
			await ctx.PlacePresenceOnTarget();

			// Defend 20
			ctx.Defend(20);

			// if you have 2 sun, 3 earth,
			if(ctx.YouHave("2 sun,3 earth" )) {

				// 3 fear if invaders are present,
				if(ctx.HasInvaders)
					ctx.AddFear(3);

				// Invaders skip all actions in target land this turn.
				ctx.GameState.SkipAllInvaderActions( ctx.Space );
			}
		}

	}
}
