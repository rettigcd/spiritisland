using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[Core.MajorCard("Power Storm",3,Speed.Fast,Element.Sun,Element.Fire,Element.Air)]
	public class PowerStorm : BaseAction {

		public PowerStorm(Spirit _,GameState gameState):base(gameState){
			ActAsync();
		}

		async Task ActAsync(){
			var spirit = await engine.SelectSpirit(gameState.Spirits);

			// target spirit gains 3 energy
			spirit.Energy += 3;

			bool hasBonus = spirit.Elements(Element.Sun) >= 2
				&& spirit.Elements(Element.Fire) >= 2
				&& spirit.Elements(Element.Air) >= 3;
			int repeats = hasBonus ? 3 : 1;
			// once this turn, target may repeat a power card by paying its cost again
			// if you have 2 sun, 2 fire, 3 air, target may repeat 2 more times by paying card their cost

		}

	} 
}
