using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class GnawingRootbiters {

		[MinorCard("Gnawing Rootbiters",0,"earth, animal")]
		[Slow]
		[FromPresence(1)]
		static public Task ActAsync(TargetSpaceCtx ctx ) {

			// push up to 2 towns
			return ctx.PushUpTo(2,Invader.Town);
		}

	}
}
