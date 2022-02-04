namespace SpiritIsland.Basegame;

public class GiftOfPower {

	[MinorCard( "Gift of Power", 0, "moon, water, earth, plant")]
	[Slow]
	[AnySpirit]
	static public Task ActAsync( TargetSpiritCtx ctx ) {
		// gain a minor power card
		return ctx.OtherCtx.DrawMinor(); 
	}

}