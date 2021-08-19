using SpiritIsland;
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

	[InnatePower( LeadTheFuriousAssult.Name, Speed.Fast )]
	[FromPresence( 0 )]
	public class LeadTheFuriousAssult_Fast {
		public const string Name = "Lead the Furious Assult";

		[InnateOption( "4 air,2 sun, 1 fire" )]
		static public Task Option1Async( TargetSpaceCtx ctx ) {
			GatherTheWarriors_Fast.RemoveSlow( ctx.Self, GatherTheWarriors.Name );
			return LeadTheFuriousAssult.Option1Async(ctx);
		}

		[InnateOption( "4 air, 4 sun, 3 fire" )]
		static public Task Option2Async( TargetSpaceCtx ctx ) {
			GatherTheWarriors_Fast.RemoveSlow( ctx.Self, GatherTheWarriors.Name );
			return LeadTheFuriousAssult.Option2Async( ctx );
		}

	}


}
