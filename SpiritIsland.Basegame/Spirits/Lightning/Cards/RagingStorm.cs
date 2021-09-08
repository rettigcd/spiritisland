
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {
	
	public class RagingStorm {
		public const string Name = "Raging Storm";

		[SpiritCard(RagingStorm.Name,3,Speed.Slow,Element.Fire,Element.Air,Element.Water)]
		[FromPresence(1)]
		static public async Task ActAsync(TargetSpaceCtx ctx){
			var grp = ctx.PowerInvaders;

			// 1 damange to each invader.
			await grp.ApplyDamageToAllTokensOfType(1, grp.Counts.Invaders());
		}

	}

}
