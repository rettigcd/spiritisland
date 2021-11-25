using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class ReachingGrasp {

		[MinorCard( "Reaching Grasp", 0, Element.Sun, Element.Air, Element.Water )]
		[Fast]
		[AnySpirit]
		static public Task Act( TargetSpiritCtx ctx ) {

			// target spirit gets +2 range with all their Powers
			ctx.GameState.TimePasses_ThisRound.Push( new PowerApiRestorer( ctx.Other ).Restore );
			ctx.Other.RangeCalc = new TargetLandApi_ExtendRange( 2, ctx.Other.RangeCalc );

			return Task.CompletedTask;
		}

	}


}
