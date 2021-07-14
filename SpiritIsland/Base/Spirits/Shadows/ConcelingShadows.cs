using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base.Spirits.Shadows {

	class ConcealingShadows {

		[SpiritCard("Conceiling Shadows",0,Speed.Fast,Element.Moon,Element.Air)]
		static public async Task Act(ActionEngine engine){
			
			var space = await engine.Api.TargetSpace_Presence(0); // range 0
			engine.GameState.noDamageToDahan.Add(space);
		}

	}

}
