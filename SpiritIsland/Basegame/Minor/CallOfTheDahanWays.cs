using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	class CallOfTheDahanWays {

		[MinorCard("Call of the Dahan Ways",1,Speed.Slow,Element.Moon,Element.Water,Element.Animal)]
		[FromPresence(1,Target.Dahan)]
		static public Task Act(TargetSpaceCtx ctx){

			// if you have 2 moon, you may instead replace 1 town with 1 dahan
			if(ctx.Invaders.HasTown && 2 <= ctx.Self.Elements[ Element.Moon ]) {
				ctx.Adjust( InvaderSpecific.Town,-1 );
				ctx.AdjustDahan( 1 );
			} else if(ctx.Invaders.HasExplorer) {
				// replace 1 explorer with 1 dahan
				ctx.Adjust( InvaderSpecific.Explorer, -1 );
				ctx.AdjustDahan( 1 );
			}
			return Task.CompletedTask;
		}

	}

}
