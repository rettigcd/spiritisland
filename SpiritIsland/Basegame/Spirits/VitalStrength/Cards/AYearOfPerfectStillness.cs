﻿using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame.Spirits.VitalStrength {

	class AYearOfPerfectStillness {

		[SpiritCard("A Year of Perfect Stillness",3,Speed.Fast,Element.Sun,Element.Earth)]
		[FromPresence(1)]
		static public Task Act(TargetSpaceCtx ctx){
			ctx.GameState.SkipAllInvaderActions(ctx.Target);
			return Task.CompletedTask;
		}

	}
}