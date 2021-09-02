using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class TerrifyingChase {

		[SpiritCard( "Terrifying Chase", 1, Speed.Slow, Element.Sun, Element.Animal )]
		[FromPresence( 0 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			// push 2 exploeres / towns / dahan
			// push another 2 explorers / towns / dahan pers beast in target land
			int pushCount = 2 + 2 * ctx.Tokens[ BacTokens.Beast ];

			int startingInvaderCount = ctx.Tokens.InvaderTotal();

			// first push invaders
			await ctx.PowerPushUpToNTokens(pushCount,TokenType.Dahan,Invader.Explorer,Invader.Town);

			// if you pushed any invaders, 2 fear
			if( ctx.Tokens.InvaderTotal() < startingInvaderCount )
				ctx.AddFear(2);

		}

	}
}
