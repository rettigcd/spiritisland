using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base.Spirits.Shadows {

	class ConcealingShadows {

		[SpiritCard("Conceiling Shadows",0,Speed.Fast,Element.Moon,Element.Air)]
		[FromPresence(0)]
		static public async Task Act(ActionEngine engine,Space target){
			engine.GameState.noDamageToDahan.Add(target);
		}

	}

}
