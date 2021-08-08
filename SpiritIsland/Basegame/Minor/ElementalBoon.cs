using SpiritIsland.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class ElementalBoon {

		[MinorCard( "Elemental Boon", 1, Speed.Fast )]
		[TargetSpirit]
		static public async Task Act( ActionEngine engine, Space target ) {
			// Target Spirit games 3 _different_ Elements of their choice

			// if you target another spirit, you also gain the chosen elements
		}


	}
}
