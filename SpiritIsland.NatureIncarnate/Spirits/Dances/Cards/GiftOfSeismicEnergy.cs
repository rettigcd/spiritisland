namespace SpiritIsland.NatureIncarnate;

public class GiftOfSeismicEnergy {

	const string Name = "Gift of Seismic Energy";

	[SpiritCard( Name, 3, Element.Sun, Element.Fire, Element.Earth, Element.Plant ), Fast, AnySpirit]
	[Instructions( "If you target yourself, gain 3 Energy. Otherwise, target Spirit gains 1 Energy per Power Card you have in play (max. 6)." ), Artist( Artists.EmilyHancock )]
	static public Task ActAsync( TargetSpiritCtx ctx ) {
		// If you target yourself
		if(ctx.Self == ctx.Other)
			ctx.Self.Energy += 3;
		// Otherwise,
		else
			// target Spirit gains 1 Energy per Power Card you have in play (max. 6).
			ctx.Other.Energy += ctx.Self.InPlay.Count;
		return Task.CompletedTask;
	}
}
