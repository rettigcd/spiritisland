namespace SpiritIsland.JaggedEarth;

public class GiftOfNaturesConnection{
		
	[MinorCard("Gift of Nature's Connection",0),Fast,AnySpirit]
	[Instructions( "Target Spirit gains either 2 Energy or 2 of a single Element (their choice). If you target another Spirit, you gain an Element of your choice." ), Artist( Artists.MoroRogers )]
	static public async Task ActAsync( TargetSpiritCtx ctx ){

		// Target Spirit gains either 2 Energy or 2 of a single Element (their choice).
		await ctx.OtherCtx.SelectActionOption( 
			new SelfCmd("Gain 2 energy", ctx=>ctx.Self.Energy+=2),
			new SelfCmd("Gain 2 of a single element", ctx => GainEl(ctx,2))
		);

		// if you target another Spirit, you gain an Element of your choice.
		if(ctx.Self != ctx.Other)
			await GainEl(ctx,1);
	}

	static async Task GainEl( SelfCtx ctx, int count ) {
		var el = await ctx.Self.SelectElementEx($"Gain {count} of single element",ElementList.AllElements);
		ctx.Self.Elements[el]+=count;
	}

}