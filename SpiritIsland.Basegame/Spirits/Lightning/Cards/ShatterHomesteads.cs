namespace SpiritIsland.Basegame;

public class ShatterHomesteads {
	public const string Name = "Shatter Homesteads";

	[SpiritCard(ShatterHomesteads.Name,2,Element.Fire,Element.Air)]
	[Slow]
	[FromSacredSite(2)]
	static public async Task Act(TargetSpaceCtx ctx){
		// 1 fear
		ctx.AddFear( 1 );
		// Destroy 1 town
		await ctx.Invaders.Destroy( 1, Invader.Town );
	}

}