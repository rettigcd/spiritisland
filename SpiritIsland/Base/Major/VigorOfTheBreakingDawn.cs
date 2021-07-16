using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	class VigorOfTheBreakingDawn {

		[MajorCard("Vigor of the Breaking Down",3,Speed.Fast,Element.Sun,Element.Animal)]
		[FromPresence(2,Filter.Dahan)]
		public static async Task ActAsync(ActionEngine engine,Space target){
			var (spirit,gs) = engine;

			// 2 damage per dahan in target land
			gs.DamageInvaders(target,2*gs.GetDahanOnSpace(target));

			bool hasBonus = spirit.HasElements(new Dictionary<Element,int>{ 
				[Element.Sun] = 3,
				[Element.Animal] = 2
			} );
			if(hasBonus){

				// you may push up to 2 dahan.
				var pushedToLands = await engine.PushUpToNDahan(target, 2);

				// 2 damage per dahan
				foreach(var neighbor in pushedToLands )
					gs.DamageInvaders( neighbor, 2*gs.GetDahanOnSpace(neighbor) );
			}
		}

	}

}
