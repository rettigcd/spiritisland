namespace SpiritIsland.Basegame;

public class UncannyMelting {

	public const string Name = "Uncanny Melting";

	[MinorCard(UncannyMelting.Name,1, Element.Sun,Element.Moon,Element.Water)]
	[Slow]
	[FromSacredSite(1,Target.Any)]
	static public async Task ActAsync(TargetSpaceCtx ctx){

		if(ctx.HasInvaders)
			ctx.AddFear(1);

		if(ctx.HasBlight && ctx.IsOneOf( Terrain.Sand, Terrain.Wetland))
			await ctx.RemoveBlight();

	}

}