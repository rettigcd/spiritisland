
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {
	
	public class RagingStorm {
		public const string Name = "Raging Storm";

		[SpiritCard(RagingStorm.Name,3,Speed.Slow,Element.Fire,Element.Air,Element.Water)]
		[FromPresence(1)]
		static public async Task Act(TargetSpaceCtx ctx){
			var grp = ctx.InvadersOn( ctx.Target );

			// 1 damange to each invader.
			await grp.ApplyDamageToEach(1, grp.InvaderTypesPresent_Generic.ToArray() );
		}

	}

}
