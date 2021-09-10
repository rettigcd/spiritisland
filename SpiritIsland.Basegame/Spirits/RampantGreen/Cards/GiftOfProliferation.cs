﻿using SpiritIsland;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	class GiftOfProliferation {

		[SpiritCard( "Gift of Proliferation", 1, Speed.Fast, Element.Moon, Element.Plant )]
		[TargetSpirit]
		static public Task ActionAsync( TargetSpiritCtx ctx ) {
			// target spirit adds 1 presense up to 1 from their presesnse
			return ctx.Other.MakeDecisionsFor( ctx.GameState ).PlacePresence(1,Target.Any);
		}

	}
}
