using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame.Spirits.VitalStrength {

	class GuardTheHealingLand {

		[SpiritCard("Guard the Healing Land",3,Speed.Fast,Element.Water,Element.Earth,Element.Plant)]
		[FromSacredSite(1)]
		static public Task Act(TargetSpaceCtx ctx){

			// remove 1 blight
			ctx.RemoveBlight();

			// defend 4
			ctx.Defend(4);

			return Task.CompletedTask;
		}

	}

}
