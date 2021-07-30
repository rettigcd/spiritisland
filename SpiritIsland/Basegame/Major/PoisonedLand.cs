using System.Collections.Generic;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Basegame {
	class PoisonedLand {

		[MajorCard("Poisoned Land",3,Speed.Slow,Element.Earth,Element.Plant,Element.Animal)]
		[FromPresence(1)]
		static public async Task ActAsync(ActionEngine engine,Space target){
			var (_,gs) = engine;

			// Add 1 blight, destroy all dahan
			gs.AddBlight(target,1);
			gs.AddDahan(target,-engine.GameState.GetDahanOnSpace(target));

			bool hasBonus = engine.Self.Elements.Has(new Dictionary<Element,int>{
				[Element.Earth] = 3,
				[Element.Plant] = 2,
				[Element.Animal] = 2
			});
			gs.AddFear(1+(hasBonus?1:0));
			gs.DamageInvaders(target,7+(hasBonus?4:0));
		}

	}
}
