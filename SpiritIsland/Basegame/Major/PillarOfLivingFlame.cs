﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class PillarOfLivingFlame {

		[MajorCard("Pillar of Living Flame",5,Speed.Slow,Element.Fire)]
		[FromSacredSite(2)]
		static public async Task ActionAsync(ActionEngine engine,Space targetLand){
			var (self,gameState) = engine;

			// 3 fear, 5 damage
			// if you have 4 fire
			bool hasBonus = self.Elements[Element.Fire]>=4;
			// +2 fear, +5 damage
			gameState.AddFear( 3 + (hasBonus ? 2 : 0) );
			await engine.DamageInvaders(targetLand, 5 + (hasBonus ? 5 : 0));

			// if target is Jungle / Wetland, add 1 blight
			if(targetLand.Terrain.IsIn(Terrain.Jungle,Terrain.Wetland))
				gameState.AddBlight(targetLand);

		}

	}
}
