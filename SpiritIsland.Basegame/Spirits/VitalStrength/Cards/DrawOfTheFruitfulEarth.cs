﻿using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame.Spirits.VitalStrength {

	class DrawOfTheFruitfulEarth {

		[SpiritCard("Draw of the Fruitful Earth",1,Speed.Slow,Element.Earth,Element.Plant,Element.Animal)]
		[FromPresence(1)]
		static public async Task Act(TargetSpaceCtx ctx){
			await ctx.GatherUpToNTokens(ctx.Target,2,TokenType.Dahan);
			await ctx.GatherUpToNTokens(ctx.Target,2,Invader.Explorer);
		}
	}
}
