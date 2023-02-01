namespace SpiritIsland.Basegame;

[InnatePower( LeadTheFuriousAssult.Name ),SlowButFastIf("4 air")]
[FromPresence(0)]
public class LeadTheFuriousAssult {
	public const string Name = "Lead the Furious Assult";

	[DisplayOnly( "4 air", "This Power may be fast." )]
	static public Task MayBeFastAsync(TargetSpaceCtx _ ) { return null; }


	[InnateOption( "2 sun,1 fire", "Destroy 1 town for every 2 dahan in target land." )]
	static public Task Option1Async(TargetSpaceCtx ctx ) {
		// Destroy 1 town for every 2 dahan
		return ctx.Invaders
			.DestroyNOfClass(ctx.Dahan.CountAll/2, Human.Town );
	}

	[InnateOption( "4 sun,3 fire", "Destroy 1 city for every 3 dahan in target land." )]
	static public async Task Option2Async( TargetSpaceCtx ctx ) {
		// Destroy 1 city for every 3 dahan
		await ctx.Invaders
			.DestroyNOfClass( ctx.Dahan.CountAll / 3, Human.City );

		await Option1Async(ctx);
	}

}