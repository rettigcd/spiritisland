namespace SpiritIsland.JaggedEarth;

public class BoonOfReimagining {

	[SpiritCard("Boon of Reimagining", 1, Element.Moon), Slow, AnySpirit]
	[Instructions( "Target Spirit may Forget a Power Card from hand or discard. If they do, they draw 6 Minor Power Cards and gain 2 of them. If you target another Spirit, they gain 1 Energy." ), Artist( Artists.EmilyHancock )]
	public static async Task ActAsync( TargetSpiritCtx ctx ) {

		// Target Spirit may Forget a Power Card from hand or discard.
		var powerCard = await ctx.Other.Forget.ACard( null, Present.Done );

		// If they do, they draw 6 minor Power Cards and gain 2 of them.
		if( powerCard != null )
			await ctx.Other.DrawMinor(6, 2);

		// If you target another Spirit, they gain 1 Energy.
		if( ctx.Self != ctx.Other )
			ctx.Other.Energy++;
	}

}