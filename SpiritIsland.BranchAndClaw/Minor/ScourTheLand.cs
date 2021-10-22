using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class ScourTheLand {

		[MinorCard( "Scour the Land", 1, Element.Air, Element.Earth )]
		[SlowButFastIf("3 air")]
		[FromPresence(2)]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			await ctx.Invaders.Destroy(3,Invader.Town);
			await ctx.Invaders.Destroy(int.MaxValue,Invader.Explorer);

			await ctx.AddBlight(1);

		}

	}

}
