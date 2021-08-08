using SpiritIsland.Core;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {
	public class Drought {
		[MinorCard( "Drought", 1, Speed.Slow, Element.Sun, Element.Fire, Element.Earth )]
		[FromPresence(1)]
		static public async Task Act( ActionEngine engine, Space target ) {
			// Destory 3 towns.
			// 1 damage to each town/city
			// add 1 blight

			// if you have 3 sun, destory 1 city

		}

	}
}
