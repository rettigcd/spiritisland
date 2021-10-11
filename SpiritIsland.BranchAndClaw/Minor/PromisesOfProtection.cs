using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class PromisesOfProtection {

		[MinorCard( "Promises of Protection", 0, Element.Sun, Element.Earth, Element.Animal )]
		[Fast]
		[FromSacredSite( 2 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			await ctx.GatherUpToNDahan( 2 );

			ctx.GameState.ModifyRavage(ctx.Space, cfg => cfg.DahanHitpoints += 2 );

		}

	}

}
