namespace SpiritIsland.Basegame;

public class GiftOfPower {

	[MinorCard( "Gift of Power", 0, "moon, water, earth, plant"),Slow,AnySpirit]
	[Instructions( "Gain a Minor Power Card." ), Artist( Artists.JoshuaWright )]
	static public Task ActAsync( TargetSpiritCtx ctx ) {
		// gain a minor power card
		return ctx.Other.DrawMinor(); 
	}

}