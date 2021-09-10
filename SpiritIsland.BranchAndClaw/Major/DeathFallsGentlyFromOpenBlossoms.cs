using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class DeathFallsGentlyFromOpenBlossoms {

		[MajorCard("Death Falls Gently from Open Blossoms",4, Speed.Slow, Element.Moon,Element.Air,Element.Plant)]
		[FromPresence(3,Target.JungleOrSand)]
		static public async Task ActAsync(TargetSpaceCtx ctx ) {
			// 4 damage.
			await ctx.DamageInvaders(4);

			// If any invaders remain, add 1 disease
			if( ctx.Tokens.Invaders().Any())
				ctx.Tokens.Disease().Count++;

			// if 3 air and 3 plant:  
			if( ctx.YouHave("3 air,3 plant" )) {
				// 3 fear.
				ctx.AddFear(3);
				// Add 1 disease to 2 adjacent lands with invaders.
				for(int i = 0; i < 2; ++i) {
					var adjCtx = await ctx.SelectAdjacentLand($"Add disease to ({i+1} of 2)");
					adjCtx.Tokens.Disease().Count++;
				}
			}
		}

	}

}
