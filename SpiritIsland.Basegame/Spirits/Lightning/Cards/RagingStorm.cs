
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {
	
	public class RagingStorm {
		public const string Name = "Raging Storm";

		[SpiritCard(RagingStorm.Name,3,Speed.Slow,Element.Fire,Element.Air,Element.Water)]
		[FromPresence(1)]
		static public async Task ActAsync(TargetSpaceCtx ctx){

			// 1 damange to each invader.
			await ctx.Invaders.ApplyDamageToAllTokensOfType(1, ctx.Tokens.Invaders());
		}

	}

}
