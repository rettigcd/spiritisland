namespace SpiritIsland.Horizons;

public class WhisperedGuidanceThroughTheNight {

	public const string Name = "Whispered Guidance Through the Night";

	[SpiritCard(Name,0,Element.Moon,Element.Air,Element.Plant),Slow,FromPresence(Filter.Jungle,1)]
	[Instructions( "Gather up to 3 Dahan. If Invaders and Dahan are present, 1 Fear." ), Artist(Artists.MoroRogers)]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		// Gather up to 3 Dahan.
		await ctx.GatherUpToNDahan(3);
		// If Invaders and Dahan are present, 1 Fear.
		if(ctx.HasInvaders && ctx.Dahan.Any)
			await ctx.AddFear(1);
	}

}
