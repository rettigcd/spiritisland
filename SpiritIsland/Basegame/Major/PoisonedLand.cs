﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {
	class PoisonedLand {

		[MajorCard("Poisoned Land",3,Speed.Slow,Element.Earth,Element.Plant,Element.Animal)]
		[FromPresence(1)]
		static public Task ActAsync(ActionEngine engine,Space target){
			var (_,gs) = engine;

			// Add 1 blight, destroy all dahan
			gs.AddBlight(target,1);
			gs.DestoryDahan(target,engine.GameState.GetDahanOnSpace(target),DahanDestructionSource.PowerCard);

			bool hasBonus = engine.Self.Elements.Has(new Dictionary<Element,int>{
				[Element.Earth] = 3,
				[Element.Plant] = 2,
				[Element.Animal] = 2
			});
			gs.AddFear(1+(hasBonus?1:0));
			gs.DamageInvaders(target,7+(hasBonus?4:0));
			return Task.CompletedTask;
		}

	}
}
