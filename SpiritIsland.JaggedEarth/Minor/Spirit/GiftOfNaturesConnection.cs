namespace SpiritIsland.JaggedEarth;

public class GiftOfNaturesConnection{
		
	[MinorCard("Gift of Nature's Connection",0),Fast,AnySpirit]
	[Instructions( "Target Spirit gains either 2 Energy or 2 of a single Element (their choice). If you target another Spirit, you gain an Element of your choice." ), Artist( Artists.MoroRogers )]
	static public async Task ActAsync( TargetSpiritCtx ctx ){

		// Target Spirit gains either 2 Energy or 2 of a single Element (their choice).
		await Cmd.Pick1(
			new SpiritAction("Gain 2 energy", spirit=>spirit.Energy+=2),
			new SpiritAction("Gain 2 of a single element", spirit => GainEl(spirit,2))
		).ActAsync(ctx.Other);

		// if you target another Spirit, you gain an Element of your choice.
		if(ctx.Self != ctx.Other)
			await GainEl(ctx.Self,1);
	}

	static async Task GainEl( Spirit spirit, int count ) {
		var el = await spirit.SelectElementEx($"Gain {count} of single element",ElementList.AllElements);
		while(0<count--)
			spirit.Elements.Add(el);
	}

}