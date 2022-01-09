using System.Threading.Tasks;


namespace SpiritIsland.Basegame {

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
				.Destroy(ctx.Dahan.Count/2, Invader.Town );
		}

		[InnateOption( "4 sun,3 fire", "Destroy 1 city for every 3 dahan in target land." )]
		static public async Task Option2Async( TargetSpaceCtx ctx ) {
			// Destroy 1 city for every 3 dahan
			await ctx.Invaders
				.Destroy( ctx.Dahan.Count / 3, Invader.City );

			await Option1Async(ctx);
		}
	}

}
