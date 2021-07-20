using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	class SavageMawbeasts {

		// !!! THESE ARE ALL MADE UP AND WRONG
		[MinorCard("Savage Maw Beasts",0,Speed.Slow)]
		[FromPresence(1)]
		static public Task ActAsync(ActionEngine engine,Space target){
			return null;
		}

	}

}
