using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class FireInTheSky {

		[MinorCard("Fire in the Sky",1,Element.Sun,Element.Fire,Element.Air)]
		[Fast]
		[FromSacredSite(1)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {
			ctx.AddFear( 2 );
			await ctx.AddStrife();
		}

	}

}
