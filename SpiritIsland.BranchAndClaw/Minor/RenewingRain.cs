namespace SpiritIsland.BranchAndClaw;

public class RenewingRain {

	[MinorCard( "Renewing Rain", 1, Element.Water, Element.Earth, Element.Plant ),Slow,FromSacredSite( 1 )]
	[Instructions( "If target land is Jungle / Sands, remove 1 Blight. -If you have- 3 Plant: Add 1 Wilds." ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync( TargetSpaceCtx ctx ) {

		if(ctx.IsOneOf(Terrain.Jungle,Terrain.Sands)) // ??? should we be using Power Filters here?
			await ctx.RemoveBlight();

		if(await ctx.YouHave("3 plant"))
			await ctx.Wilds.AddAsync(1);

	}

}