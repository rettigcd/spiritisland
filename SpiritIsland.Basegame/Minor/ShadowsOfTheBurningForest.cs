﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class ShadowsOfTheBurningForest {

		[MinorCard("Shadows of the Burning Forest",0,Element.Moon,Element.Fire,Element.Plant)]
		[Slow]
		[FromPresence(0,Target.Invaders)]
		static public async Task Act(TargetSpaceCtx ctx){

			// 2 fear
			ctx.AddFear(2);

			// if target is M/J, Push 1 explorer and 1 town
			if(ctx.IsOneOf( Terrain.Mountain, Terrain.Jungle )) {
				await ctx.Pusher
					.AddGroup( 1, Invader.Town )
					.AddGroup( 1, Invader.Explorer )
					.MoveN();
			}

		}

	}
}
