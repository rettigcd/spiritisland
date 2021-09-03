using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class ScourTheLand {

		[MinorCard( "Scour hte Land", 1, Speed.Slow, Element.Air, Element.Earth )]
		[FromSacredSite( 2 )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			var invaders = ctx.InvadersOn( ctx.Target );
			await invaders.Destroy(3,Invader.Town);
			await invaders.Destroy(int.MaxValue,Invader.Explorer);
			ctx.AddBlight(1);
		}


	}
}
