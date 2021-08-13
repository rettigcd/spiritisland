using SpiritIsland.Core;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class ElementalBoon {

		[MinorCard( "Elemental Boon", 1, Speed.Fast )]
		[TargetSpirit]
		static public async Task Act( ActionEngine engine, Space target ) {
			// Target Spirit games 3 _different_ Elements of their choice

			// if you target another spirit, you also gain the chosen elements


			// !!!! Create an ElementPool that we can push elements into, then flush at the end of the round
		}


	}
}
