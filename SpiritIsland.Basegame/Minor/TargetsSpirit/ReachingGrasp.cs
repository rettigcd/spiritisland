using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class ReachingGrasp {

		[MinorCard( "Reaching Grasp", 0, Speed.Fast, Element.Sun, Element.Air, Element.Water )]
		[TargetSpirit]
		static public Task Act( TargetSpiritCtx ctx ) {
			// target spirit gets +2 range with all their Powers
			TargetLandApi.ScheduleRestore( ctx );
			TargetLandApi.ExtendRange(ctx.Self,2);

			return Task.CompletedTask;
		}

	}


}
