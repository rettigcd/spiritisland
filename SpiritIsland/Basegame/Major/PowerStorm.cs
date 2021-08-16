using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class PowerStorm {

		[MajorCard("Powerstorm",3,Speed.Fast,Element.Sun,Element.Fire,Element.Air)]
		[TargetSpirit]
		static public async Task ActionAsync(ActionEngine engine,Spirit target){
			
			// target spirit gains 3 energy
			target.Energy += 3;

			// once this turn, target may repeat a power card by paying its cost again
			// if you have 2 sun, 2 fire, 3 air, target may repeat 2 more times by paying card their cost
			int repeats = target.Elements.Contains("2 sun,2 fire,3 air") ? 3 : 1;

			// Taret spirit may use up to 2 slow powers as if they were fast powers this turn.
			await engine.SelectActionsAndMakeFast( target, repeats );
			// !!! need to pay for it again
		}

	}
}
