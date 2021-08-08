using SpiritIsland.Core;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class VeilTheNightsHunt {

		[MinorCard( "Veil the Night's Hunt", 1, Speed.Fast, Element.Moon, Element.Air, Element.Animal]
		[FromPresence( 2, Filter.Dahan )]
		static public async Task Act( ActionEngine engine, Space target ) {
			// each dahan deals 1 damage to a different invader
			// - or -
			// push up to 3 dahan
		}

	}
}
