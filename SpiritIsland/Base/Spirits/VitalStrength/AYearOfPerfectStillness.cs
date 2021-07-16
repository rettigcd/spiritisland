using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base.Spirits.VitalStrength {

	class AYearOfPerfectStillness {

		[SpiritCard("A Year of Perfect Stillness",3,Speed.Fast,Element.Sun,Element.Earth)]
		[FromPresence(1)]
		static public async Task Act(ActionEngine eng,Space target){
			eng.GameState.SkipAllInvaderActions(target);
			// !!! doesn't clear following invader phase
		}

	}
}
