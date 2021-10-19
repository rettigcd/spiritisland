using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class RegrowFromRoots {

		[SpiritCard("Regrow from Roots",1,Element.Water,Element.Plant)]
		[Slow]
		[FromPresence(1,Target.JungleOrWetland)]
		static public async Task ActAsync( TargetSpaceCtx ctx ) {

			// if ther are 2 blight or fewer in target land, remove 1 blight
			if( ctx.Blight <= 2)
				await ctx.RemoveBlight();

		}

	}

}
