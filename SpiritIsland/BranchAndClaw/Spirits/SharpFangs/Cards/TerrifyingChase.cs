using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class TerrifyingChase {

		[SpiritCard( "Terrifying Chase", 1, Speed.Slow, Element.Sun, Element.Animal )]
		[FromPresence( 0 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {
			// push 2 exploeres / towns / dahan
			// push another 2 explorers / towns / dahan pers beast in target land
			int pushCount = 2 + 2 * ctx.GameState.BAC().Beasts.GetCount(ctx.Target);

			// first push invaders
			var invaderPushCount = await ctx.PowerPushUpToNInvaders(pushCount,Invader.Explorer,Invader.Town);

			// if you pushed any invaders, 2 fear
			if(invaderPushCount>0)
				ctx.AddFear(2);

			// left over counts, use for dahan
			await ctx.PowerPushUpToNDahan(pushCount-invaderPushCount);

		}

	}
}
