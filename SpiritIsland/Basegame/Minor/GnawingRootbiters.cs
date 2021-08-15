using SpiritIsland.Core;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class GnawingRootbiters {

		[MinorCard("Gnawing Rootbiters",0,Speed.Slow,"earth, animal")]
		[FromPresence(1)]
		static public Task ActAsync(ActionEngine eng, Space target ) {
			// push up to 2 towns
			return eng.PushUpToNInvaders(target,2,Invader.Town);
		}

	}
}
