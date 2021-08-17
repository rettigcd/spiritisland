﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class VoraciousGrowth {

		[MinorCard("Voracious Growth",1,Speed.Slow,Element.Water,Element.Plant)]
		[FromSacredSite(1,Target.JungleOrWetland)]
		static public async Task ActAsync(ActionEngine engine,Space target){
			var (_,gs) = engine;

			const string blightKey = "Remove 1 Blight";

			bool removeBlight = gs.HasBlight(target) 
				&& blightKey == await engine.SelectText("Select action","2 Damage",blightKey);

			if(removeBlight)
				gs.AddBlight(target,-1);
			else
				// 2 damage
				await engine.DamageInvaders(target,2);
		}

	}

}
