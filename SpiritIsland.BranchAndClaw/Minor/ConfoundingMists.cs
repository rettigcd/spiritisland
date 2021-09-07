﻿using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class ConfoundingMists {

		[MinorCard( "Confounding Mists", 1, Speed.Fast, Element.Air, Element.Water )]
		[FromPresence( 1 )]
		static public Task ActAsync( TargetSpaceCtx ctx ) {
			return ctx.SelectActionOption(
				new ActionOption("Defend 4", ()=>ctx.Defend(4)),
				new ActionOption("Invaders added to target are immediately pushed", ()=>PushFutureInvadersFromLands(ctx))
			);
		}

		static Task PushFutureInvadersFromLands( TargetSpaceCtx _ ) {

			// each invader added to target land this turn may be immediatley pushed to any adjacent land
			// !!!

			return Task.CompletedTask;
		}
	}

}
