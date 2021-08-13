﻿using SpiritIsland.Core;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame.Spirits.RampantGreen {

	class GiftOfProliferation {

		[SpiritCard( "Gift of Proliferation", 1, Speed.Fast, Element.Moon, Element.Plant )]
		[TargetSpirit]
		static public Task ActionAsync( ActionEngine eng, Spirit target ) {
			// target spirit adds 1 presense up to 1 from their presesnse
			return new ActionEngine(target,eng.GameState).PlacePresence(1,Filter.None);
		}

	}
}
