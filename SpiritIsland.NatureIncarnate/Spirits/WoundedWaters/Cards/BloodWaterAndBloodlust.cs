namespace SpiritIsland.NatureIncarnate;

public class BloodWaterAndBloodlust {

	[SpiritCard( "Blood Water and Bloodlust", 1, Element.Fire, Element.Water, Element.Animal ), Slow]
	[FromPresenceIn( Target.Blight, 1 )]
	[Instructions( "Add 1 Beast and 1 Disease." ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {
		await ctx.Beasts.AddAsync(1);
		await ctx.Disease.AddAsync(1);
	}

}

