using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class TooNearTheJungle {

		[SpiritCard( "Too Near the Jungle", 0, Speed.Slow, Element.Plant, Element.Animal )]
		[FromPresenceIn( 1, Terrain.Jungle )]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			ctx.AddFear(1);
			await ctx.Invaders.DestroyAny(1,Invader.Explorer);
		}


	}
}
