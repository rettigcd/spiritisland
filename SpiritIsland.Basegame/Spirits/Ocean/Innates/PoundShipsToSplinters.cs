using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	[InnatePower( PoundShipsToSplinters.Name ),Fast]
	[FromPresence( 0, Target.Coastal )]
	public class PoundShipsToSplinters {

		public const string Name = "Pound Ships to Splinters";

		[InnateOption( "1 moon,1 air,2 water","1 fear." )]
		static public Task Option1( TargetSpaceCtx ctx ) {
			// 1 fear
			ctx.AddFear( 1 );
			return Task.CompletedTask;
		}

		[InnateOption( "2 moon,1 air,3 water","+1 fear." )]
		static public Task Option2( TargetSpaceCtx ctx ) {
			//+1 fear
			ctx.AddFear( 1 );
			return Option1( ctx );
		}

		[InnateOption( "3 moon,2 air,4 water", "+2 fear." )]
		static public Task Option3( TargetSpaceCtx ctx ) {
			//+2 fear
			ctx.AddFear( 2 );
			return Option2( ctx );
		}


	}

}
