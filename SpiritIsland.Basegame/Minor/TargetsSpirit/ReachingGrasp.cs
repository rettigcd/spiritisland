using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class ReachingGrasp {

		[MinorCard( "Reaching Grasp", 0, Element.Sun, Element.Air, Element.Water )]
		[Fast]
		[TargetSpirit]
		static public Task Act( TargetSpiritCtx ctx ) {

			// target spirit gets +2 range with all their Powers
			TargetLandApi.ScheduleRestore( ctx.OtherCtx );
			TargetLandApi.ExtendRange( ctx.Other, 2 );

			return Task.CompletedTask;
		}

	}


}
