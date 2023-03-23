namespace SpiritIsland.Basegame;

public class GiftOfProliferation {

	[SpiritCard( "Gift of Proliferation", 1, Element.Moon, Element.Plant ),Fast,AnotherSpirit]
	[Instructions( "Target Spirit adds 1 Presence up to 1 Range from their Presence." ),Artist( Artists.JorgeRamos )]
	static public Task ActionAsync( TargetSpiritCtx ctx ) {
		// target spirit adds 1 presense up to range 1 from their presesnse
		return ctx.Other.PlacePresenceWithin( new TargetCriteria( 1 ), true);
	}

}