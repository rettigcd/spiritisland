﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class DarkAndTangledWoods {

		[MinorCard("Dark and Tangled Woods", 1, Speed.Fast, Element.Moon, Element.Earth, Element.Plant)]
		[FromPresence(1)]
		static public Task Act(TargetSpaceCtx ctx){
			// 2 fear
			ctx.AddFear(2);

			// if target is M/J, Defend 3
			if(ctx.IsOneOf(Terrain.Jungle,Terrain.Mountain))
				ctx.Defend(3);
			return Task.CompletedTask;
		}

	}
}
