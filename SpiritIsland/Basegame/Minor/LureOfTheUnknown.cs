﻿using SpiritIsland.Core;

namespace SpiritIsland.Basegame {
	public class LureOfTheUnknown {
		[MinorCard( "Lure of the Unknown", 0, Speed.Fast, Element.Moon, Element.Fire, Element.Air, Element.Plant )]
		[FromPresence( 2, Filter.NoInvader )]
		public static Task ActAsync( ActionEngine eng, Space target ) {
			return eng.GatherUpToNInvaders( target, 1, Invader.Explorer, Invader.Town );
		}
	}


}
