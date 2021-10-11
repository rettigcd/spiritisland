﻿using System.Threading.Tasks;


namespace SpiritIsland.Basegame {

	[InnatePower( LeadTheFuriousAssult.Name,Speed.Slow)]
	[FromPresence(0)]
	public class LeadTheFuriousAssult {
		public const string Name = "Lead the Furious Assult";

		[InnateOption( "4 air", "This Power may be fast.", AttributePurpose.DisplayOnly )]
		static public Task MayBeFastAsync(TargetSpaceCtx _ ) { return null; }


		[InnateOption( "2 sun,1 fire", "Destory 1 town for every 2 dahan in target land." )]
		static public Task Option1Async(TargetSpaceCtx ctx ) {
			// Destroy 1 town for every 2 dahan
			return ctx.Invaders
				.Destroy(ctx.DahanCount/2, Invader.Town );
		}

		[InnateOption( "4 sun,3 fire", "Destory 1 city for every 3 dahan in target land." )]
		static public async Task Option2Async( TargetSpaceCtx ctx ) {
			// Destroy 1 city for every 3 dahan
			await ctx.Invaders
				.Destroy( ctx.DahanCount / 3, Invader.City );

			await Option1Async(ctx);
		}
	}

}
