using SpiritIsland.Core;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class ReachingGrasp {

		[MinorCard( "Reaching Grasp", 0, Speed.Fast, Element.Sun, Element.Air, Element.Water)]
		[TargetSpirit]
		static public async Task Act( ActionEngine engine, Spirit target ) {
			// target spirit gets +2 range with all their Powers

			// for this turn, replace target spirits PowerCard Api with ine that extends range by 2

		}
	}
}
