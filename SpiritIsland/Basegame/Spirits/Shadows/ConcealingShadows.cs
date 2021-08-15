using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	class ConcealingShadows {

		[SpiritCard("Concealing Shadows",0,Speed.Fast,Element.Moon,Element.Air)]
		[FromPresence(0)]
		static public Task Act(ActionEngine engine,Space target){
			engine.GameState.noDamageToDahan.Add(target);
			return Task.CompletedTask;
		}

	}

}
