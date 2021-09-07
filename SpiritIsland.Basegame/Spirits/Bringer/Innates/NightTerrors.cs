using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	[InnatePower( NightTerrors.Name, Speed.Fast )]
	[FromPresence( 0, Target.Invaders )]
	public class NightTerrors {

		public const string Name = "Night Terrors";

		[InnateOption( "1 moon,1 air" )]
		static public Task Option1Async( TargetSpaceCtx ctx ) {
			// 1 fear
			ctx.AddFear(1);
			return Task.CompletedTask;
		}

		[InnateOption( "2 moon,1 air,1 animal")]
		static public Task Option2Async( TargetSpaceCtx ctx ) {
			//+1 fear
			ctx.AddFear( 2 );
			return Task.CompletedTask;
		}

		[InnateOption("3 moon,2 air,1 animal")]
		static public Task Option3Async( TargetSpaceCtx ctx ) {
			//+1 fear
			ctx.AddFear( 3 );
			return Task.CompletedTask;
		}


	}

}
