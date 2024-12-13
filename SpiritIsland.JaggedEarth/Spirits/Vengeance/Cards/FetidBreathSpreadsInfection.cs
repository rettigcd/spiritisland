namespace SpiritIsland.JaggedEarth;

public class FetidBreathSpreadsInfection {

	[SpiritCard("Fetid Breath Spreads Infection",2,Element.Air,Element.Water,Element.Animal), Slow, FromPresence(1,Filter.Invaders)]
	[Instructions( "1 Fear. Add 1 Disease." ), Artist( Artists.DamonWestenhofer )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		await ctx.AddFear(1);
		await ctx.Disease.AddAsync(1);
	}

}