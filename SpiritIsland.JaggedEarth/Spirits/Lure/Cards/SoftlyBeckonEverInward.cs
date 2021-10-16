using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class SoftlyBeckonEverInward {

		[SpiritCard("Softly Beckon Ever Inward",2,Element.Moon,Element.Air),Slow,FromPresence(0,Target.Inland)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {
			// gather up to 2 explorers
			await ctx.GatherUpTo(2,Invader.Explorer);
			// gather up to 2 towns
			await ctx.GatherUpTo(2,Invader.Town);
			// gather up to 2 beast
			await ctx.GatherUpTo(2,TokenType.Beast.Generic);
			// gather up to 2 dahan
			await ctx.GatherUpToNDahan(2);
		}
	}

}
