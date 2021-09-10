using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class FavorsCalledDue {

		[SpiritCard("Favors Called Due",1,Speed.Slow,Element.Moon,Element.Air,Element.Animal)]
		[FromPresence(1)]
		static public async Task Act(TargetSpaceCtx ctx){

			// gather up to 4 dahan
			await ctx.GatherUpToNDahan( 4 );

			// if invaders are present and dahan now out numberthem, 3 fear
			var invaderCount = ctx.Tokens.InvaderTotal();
			if(0 < invaderCount && invaderCount < ctx.DahanCount)
				ctx.AddFear( 3 );

		}
	}

}
