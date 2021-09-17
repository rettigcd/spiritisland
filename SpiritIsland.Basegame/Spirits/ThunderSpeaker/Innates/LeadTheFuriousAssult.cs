using System.Threading.Tasks;


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
				.Destroy(ctx.GameState.DahanGetCount(ctx.Space)/2, Invader.Town );
		}

		[InnateOption( "4 sun,3 fire", "Destory 1 city for every 3 dahan in target land." )]
		static public async Task Option2Async( TargetSpaceCtx ctx ) {
			// Destroy 1 city for every 3 dahan
			await ctx.Invaders
				.Destroy( ctx.GameState.DahanGetCount( ctx.Space ) / 3, Invader.City );

			await Option1Async(ctx);
		}
	}

	class FastIf4Air<T> : InnatePower_TargetSpace {
		public FastIf4Air() : base( typeof( T ) ) { }

		public override void UpdateFromSpiritState( CountDictionary<Element> elements ) {
			base.UpdateFromSpiritState( elements );
			OverrideSpeed = elements.Contains("4 air") ? new SpeedOverride( Speed.FastOrSlow, LeadTheFuriousAssult.Name )  : null;
		}

	}

}
