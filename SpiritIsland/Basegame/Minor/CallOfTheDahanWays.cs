
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class CallOfTheDahanWays {

		[MinorCard("Call of the Dahan Ways",1,Speed.Slow,Element.Moon,Element.Water,Element.Animal)]
		[FromPresence(1,Target.Dahan)]
		static public Task Act(TargetSpaceCtx ctx){
			var target = ctx.Target;
			var grp = ctx.GameState.InvadersOn(target);

			// if you have 2 moon, you may instead replace 1 town with 1 dahan
			if(grp.HasTown && 2 <= ctx.Self.Elements[ Element.Moon ]) {
				ctx.GameState.Adjust(target,InvaderSpecific.Town,-1);
				ctx.GameState.AdjustDahan(target,1);
			} else if( grp.HasExplorer) {
				// replace 1 explorer with 1 dahan
				ctx.GameState.Adjust( target, InvaderSpecific.Explorer, -1 );
				ctx.GameState.AdjustDahan( target, 1 );
			}
			return Task.CompletedTask;
		}

	}

}
