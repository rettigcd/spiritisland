using System;

namespace SpiritIsland.Basegame;

public class GiftOfProliferation {

	[SpiritCard( "Gift of Proliferation", 1, Element.Moon, Element.Plant ),Fast,AnotherSpirit]
	static public Task ActionAsync( TargetSpiritCtx ctx ) {
		// target spirit adds 1 presense up to range 1 from their presesnse
		return ctx.OtherCtx.Presence.PlaceWithin( new TargetCriteria( 1 ), true);
	}

}