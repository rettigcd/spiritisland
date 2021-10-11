﻿using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class GiftOfPower {

		[MinorCard( "Gift of Power", 0, "moon, water, earth, plant")]
		[Slow]
		[TargetSpirit]
		static public Task ActAsync( TargetSpiritCtx ctx ) {
			// gain a minor power card
			return ctx.OtherCtx.DrawMinor(); 
		}

	}
}
