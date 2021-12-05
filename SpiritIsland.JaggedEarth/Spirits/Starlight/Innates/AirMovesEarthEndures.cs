using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	[InnatePower("Air Moves, Earth Endures"), Fast, FromPresence(1)]
	class AirMovesEarthEndures {

		[InnateOption("3 air","Push up to 2 explorer or 1 town.")]
		static public Task Option1(TargetSpaceCtx ctx ) {
			var push2Explorers = new ActionOption("Push up to 2 explorers", ()=> ctx.PushUpTo(2,Invader.Explorer) );
			var push1Town = new ActionOption( "Push up to 1 town", () => ctx.PushUpTo( 1, Invader.Town ) );
			return ctx.SelectActionOption( push2Explorers,push1Town );
		}

		[InnateOption("3 earth","Defend 5.",1)]
		static public Task Option2(TargetSpaceCtx ctx ) {
			ctx.Defend( 5 );
			return Task.CompletedTask;
		}

	}


}
