﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class ShadowsOfTheBurningForest {

		[MinorCard("Shadows of the Burning Forest",0,Speed.Slow,Element.Moon,Element.Fire,Element.Plant)]
		[FromPresence(0,Target.Invaders)]
		static public async Task Act(TargetSpaceCtx ctx){
			var target = ctx.Target;
			// 2 fear
			ctx.AddFear(2);

			// if target is M/J, Push 1 explorer and 1 town
			if(target.Terrain.IsIn( Terrain.Mountain, Terrain.Jungle )) {
				// Push Town
				await ctx.PushUpToNInvaders(target,1,Invader.Town);
				// Push Explorer
				await ctx.PushUpToNInvaders( target, 1, Invader.Explorer );
			}

		}

	}
}
