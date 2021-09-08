using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class ScourTheLand {

		[MinorCard( "Scour hte Land", 1, Speed.Slow, Element.Air, Element.Earth )]
		[FromSacredSite( 2 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			await ctx.PowerInvaders.Destroy(3,Invader.Town);
			await ctx.PowerInvaders.Destroy(int.MaxValue,Invader.Explorer);
			ctx.AddBlight(1);
		}


	}
}
