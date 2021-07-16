using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	public class PowerStorm {

		[MajorCard("Power Storm",3,Speed.Fast,Element.Sun,Element.Fire,Element.Air)]
		[TargetSpirit]
		static public async Task ActionAsync(ActionEngine engine,Spirit target){
			
			// target spirit gains 3 energy
			target.Energy += 3;

			bool hasBonus = target.Elements(Element.Sun) >= 2
				&& target.Elements(Element.Fire) >= 2
				&& target.Elements(Element.Air) >= 3;
			int repeats = hasBonus ? 3 : 1;
			// once this turn, target may repeat a power card by paying its cost again
			// if you have 2 sun, 2 fire, 3 air, target may repeat 2 more times by paying card their cost

			// Taret spirit may use up to 2 slow powers as if they were fast powers this turn.
			await engine.SelectActionsAndMakeFast( target, repeats );
		}

	}
}
