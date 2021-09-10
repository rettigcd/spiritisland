using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class ConcealingShadows {

		[SpiritCard("Concealing Shadows",0,Speed.Fast,Element.Moon,Element.Air)]
		[FromPresence(0)]
		static public Task Act(TargetSpaceCtx ctx){
			ctx.GameState.ModifyRavage( ctx.Space, cfg=>cfg.ShouldDamageDahan = false );
			return Task.CompletedTask;
		}

	}

}
