using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiritIsland.PowerCards {
	public class WashAway : PowerCard {

		public WashAway():base("Wash Away", 1, Speed.Slow
			,Element.Water
			,Element.Earth
		){}

		// target: range 1

		// push up to 3 explorers / towns

	}
}
