using SpiritIsland.Core;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	//1 water 3 plant defend 2
	//2 water 4 plant instead, defend 4
	//3 water 1 rock 5 plant also, remove 1 blight

	[InnatePower("All Enveloping Green",Speed.Fast)]
	[FromSacredSite(1)]
	class AllEnvelopingGreen {

		[InnateOption("1 water, 3 plant")]
		static public Task Option1Async( ActionEngine engine, Space target ) {
			//defend 2
			engine.GameState.Defend( target, 2 );
			return Task.CompletedTask;
		}

		[InnateOption( "2 water, 4 plant" )]
		static public Task Option2Async( ActionEngine engine, Space target ) {
			//defend 4 (instead)
			engine.GameState.Defend(target,4);
			return Task.CompletedTask;
		}

		[InnateOption( "3 water, 5 plant, 1 earth" )]
		static public Task Option3Async( ActionEngine engine, Space target ) {
			Option2Async(engine,target);
			// also remove 1 blight
			engine.GameState.RemoveBlight(target);
			return Task.CompletedTask;
		}

	}
}