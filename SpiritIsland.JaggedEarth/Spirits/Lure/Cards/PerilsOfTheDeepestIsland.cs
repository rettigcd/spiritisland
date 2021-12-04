using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class PerilsOfTheDeepestIsland {

		[SpiritCard("Perils of the Deepest Island",1,Element.Moon,Element.Plant,Element.Animal),Slow,FromPresence(0,Target.Inland)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {

			// 1 fear.
			ctx.AddFear(1);

			// add 1 badlands.
			ctx.Badlands.Count++;  

			// Add 1 beast within 1 range.
			var spaceCtx = await ctx.SelectSpace("Add beast", ctx.FindSpacesWithinRangeOf(1,Target.Any));
			spaceCtx.Beasts.Count++;

			// Push up to 2 dahan.
			await ctx.PushUpToNDahan(2);
		}
	}

}
