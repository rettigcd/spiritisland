using SpiritIsland;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	//1 water 3 plant defend 2
	//2 water 4 plant instead, defend 4
	//3 water 1 rock 5 plant also, remove 1 blight

	[InnatePower("All Enveloping Green",Speed.Fast)]
	[FromSacredSite(1)]
	class AllEnvelopingGreen {

		[InnateOption("1 water, 3 plant")]
		static public Task Option1Async( TargetSpaceCtx ctx ) {
			//defend 2
			ctx.GameState.Defend( ctx.Target, 2 );
			return Task.CompletedTask;
		}

		[InnateOption( "2 water, 4 plant" )]
		static public Task Option2Async( TargetSpaceCtx ctx ) {
			//defend 4 (instead)
			ctx.GameState.Defend(ctx.Target,4);
			return Task.CompletedTask;
		}

		[InnateOption( "3 water, 5 plant, 1 earth" )]
		static public Task Option3Async( TargetSpaceCtx ctx ) {
			Option2Async(ctx);
			// also remove 1 blight
			ctx.GameState.RemoveBlight(ctx.Target);
			return Task.CompletedTask;
		}

	}
}