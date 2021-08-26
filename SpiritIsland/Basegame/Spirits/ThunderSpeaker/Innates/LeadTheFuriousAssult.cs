using System.Threading.Tasks;


namespace SpiritIsland.Basegame {

	[InnatePower(LeadTheFuriousAssult.Name,Speed.Slow)]
	[FromPresence(0)]
	public class LeadTheFuriousAssult {
		public const string Name = "Lead the Furious Assult";

		[InnateOption( "2 sun, 1 fire" )]
		static public Task Option1Async(TargetSpaceCtx ctx ) {
			// Destroy 1 town for every 2 dahan
			return ctx.InvadersOn(ctx.Target)
				.Destroy(Invader.Town,ctx.GameState.DahanCount(ctx.Target)/2);
		}

		[InnateOption( "4 sun, 3 fire" )]
		static public async Task Option2Async( TargetSpaceCtx ctx ) {
			// Destroy 1 city for every 3 dahan
			await ctx.InvadersOn( ctx.Target )
				.Destroy( Invader.City, ctx.GameState.DahanCount( ctx.Target ) / 3 );

			await Option1Async(ctx);
		}
	}

	class FastIf4Air<T> : InnatePower_TargetSpace {
		public FastIf4Air() : base( typeof( T ) ) { }

		public override bool UpdateAndISActivatedBy( CountDictionary<Element> elements ) {
			Speed = 4<=elements[Element.Air] ? Speed.FastOrSlow : Speed.Slow; // SIDE EFFECT!
			return base.UpdateAndISActivatedBy( elements );
		}
	}

}
