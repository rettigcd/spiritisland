using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base.Spirits.VitalStrength {

	class AYearOfPerfectStillness {

		[SpiritCard("A Year of Perfect Stillness",3,Speed.Fast,Element.Sun,Element.Earth)]
		static public async Task Act(ActionEngine eng){

			var target = await eng.Api.TargetSpace_Presence(1);
			// invaders skip all actions in target land

			eng.GameState.SkipAllInvaderActions(target);

			// !!! doesn't clear following invader phase
		}

	}
}
