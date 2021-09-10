using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class ConcealingShadows {

		[SpiritCard("Concealing Shadows",0,Speed.Fast,Element.Moon,Element.Air)]
		[FromPresence(0)]
		static public Task Act(TargetSpaceCtx ctx){
			// 1 fear
			ctx.AddFear(1);

			// dahan take no damage from raving invaders this turn
			ctx.GameState.ModifyRavage( ctx.Space, cfg=>cfg.ShouldDamageDahan = false );

			return Task.CompletedTask;
		}

	}

}
