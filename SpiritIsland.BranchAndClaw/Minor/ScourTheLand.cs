using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class ScourTheLand {

		[MinorCard( "Scour the Land", 1, Speed.Slow, Element.Air, Element.Earth )]
		[FromSacredSite( 2 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			await ctx.Invaders.Destroy(3,Invader.Town);
			await ctx.Invaders.Destroy(int.MaxValue,Invader.Explorer);

			ctx.AddBlight(1);

			// !!! if you have 3 air, thie power may be fast

		}


	}
}
