namespace SpiritIsland.Basegame;

public class UncannyMelting {

	public const string Name = "Uncanny Melting";

	[MinorCard(UncannyMelting.Name,1, Element.Sun,Element.Moon,Element.Water),Slow,FromSacredSite(1,Filter.Any)]
	[Instructions( "If Invaders are present, 1 Fear. If target land is Sands / Wetland, remove 1 Blight" ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync(TargetSpaceCtx ctx){

		if(ctx.HasInvaders)
			await ctx.AddFear(1);

		if(ctx.HasBlight && ctx.IsOneOf( Terrain.Sands, Terrain.Wetland))
			await ctx.RemoveBlight();

	}

}