using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base.Spirits.VitalStrength {

	class GuardTheHealingLand {

		[SpiritCard("Guard the Healing Land",3,Speed.Fast,Element.Water,Element.Earth,Element.Plant)]
		static public async Task Act(ActionEngine eng){
			var target = await eng.Api.TargetSpace_SacredSite(1);
			
			// remove 1 blight, defend 4
			eng.GameState.Defend(target,4);

			if(eng.GameState.GetBlightOnSpace(target)>0)
				eng.GameState.AddBlight(target,-1);

		}

	}

}
