using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame.Spirits.VitalStrength {

	class GuardTheHealingLand {

		[SpiritCard("Guard the Healing Land",3,Speed.Fast,Element.Water,Element.Earth,Element.Plant)]
		[FromSacredSite(1)]
		static public Task Act(TargetSpaceCtx ctx){
			
			// remove 1 blight, defend 4
			ctx.GameState.Defend(ctx.Space,4);

			if(ctx.GameState.GetBlightOnSpace(ctx.Space)>0)
				ctx.GameState.AddBlight(ctx.Space,-1);
			return Task.CompletedTask;
		}

	}

}
