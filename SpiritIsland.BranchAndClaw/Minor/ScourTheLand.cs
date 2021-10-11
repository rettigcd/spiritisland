using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class ScourTheLand {

		[MinorCardConditinalFast( "Scour the Land", 1, "3 air", Element.Air, Element.Earth )] // !!!
		[Slow]
		[FromSacredSite( 2 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			await ctx.Invaders.Destroy(3,Invader.Town);
			await ctx.Invaders.Destroy(int.MaxValue,Invader.Explorer);

			ctx.AddBlight(1);

		}

	}

}
