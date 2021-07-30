﻿using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {

	class DarkAndTangledWoods {

		[MinorCard("Dark and Tangled Woods", 1, Speed.Fast, Element.Moon, Element.Earth, Element.Plant)]
		[FromPresence(1)]
		static public async Task Act(ActionEngine eng,Space target){
			// 2 fear
			eng.GameState.AddFear(2);

			// if target is M/J, Defend 3
			if(target.Terrain.IsIn(Terrain.Jungle,Terrain.Mountain))
				eng.GameState.Defend(target,3);
		}

	}
}
