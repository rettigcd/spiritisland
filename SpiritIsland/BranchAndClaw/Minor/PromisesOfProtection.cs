using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class PromisesOfProtection {

		[MinorCard( "Promises of Protection", 0, Speed.Fast, Element.Sun, Element.Earth, Element.Animal )]
		[FromSacredSite( 2 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			await ctx.GatherUpToNDahan( 2 );

			ctx.GameState.ModifyRavage(ctx.Target, cfg => cfg.DahanHitpoints+=2 );

		}

	}

}
