using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	class FavorsCalledDue {

		[SpiritCard("Favors Called Due",1,Speed.Slow,Element.Moon,Element.Air,Element.Animal)]
		[FromPresence(1)]
		static public async Task Act(TargetSpaceCtx ctx){
			var target = ctx.Target;
			var (_,gameState) = ctx;

			// gather up to 4 dahan
			await ctx.GatherUpToNTokens( target, 4, TokenType.Dahan );

			// if invaders are present and dahan now out numberthem, 3 fear
			var invaderCount = ctx.Tokens.InvaderTotal();
			if(invaderCount > 0 && gameState.DahanGetCount( target ) > invaderCount) {
				ctx.AddFear( 3 );
			}

		}
	}

}
