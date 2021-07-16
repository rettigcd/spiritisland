﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {
	class PoisonedLand {

		[MajorCard("Poisoned Land",3,Speed.Slow,Element.Earth,Element.Plant,Element.Animal)]
		static public async Task ActAsync(ActionEngine engine){
			var (_,gs) = engine;
			var target = await engine.Api.TargetSpace_Presence(1);

			// Add 1 blight, destroy all dahan
			gs.AddBlight(target,1);
			gs.AddDahan(target,-engine.GameState.GetDahanOnSpace(target));

			bool hasBonus = engine.Self.HasElements(new Dictionary<Element,int>{
				[Element.Earth] = 3,
				[Element.Plant] = 2,
				[Element.Animal] = 2
			});
			gs.AddFear(1+(hasBonus?1:0));
			gs.DamageInvaders(target,7+(hasBonus?4:0));
		}

	}
}