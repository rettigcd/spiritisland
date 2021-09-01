using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	[InnatePower("Raging Hunt",Speed.Fast)]
	[FromPresence(1,Target.NoBlight)]
	public class RagingHunt {

		[InnateOption("2 animal")]
		static public async Task Option1(TargetSpaceCtx ctx ) {
			await ctx.GatherUpToNBeasts(ctx.Target,1);
			await ctx.PowerPushUpToNBeasts(ctx.Target,2);
		}

		[InnateOption( "2 plant,3 animal" )]
		static public async Task Option2( TargetSpaceCtx ctx ) {
			await ctx.GatherUpToNBeasts( ctx.Target, 1 );
			await ctx.DamageInvaders( ctx.GameState.BAC().Beasts.GetCount(ctx.Target) );
			await ctx.PowerPushUpToNBeasts( ctx.Target, 2 );
		}

	}

}
