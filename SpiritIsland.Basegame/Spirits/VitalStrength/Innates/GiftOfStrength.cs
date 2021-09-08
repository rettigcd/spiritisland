using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	[InnatePower("Gift of Strength",Speed.Fast)]
	[TargetSpirit]
	public class GiftOfStrength {

		// * Note * these have a different signature than other Innates, called directly from GiftOfStrength_InnatePower

		[InnateOption("1 sun,2 earth,2 plant")]
		static public Task Option1( TargetSpiritCtx ctx ) {
			// Once this turn, Target Spirit may Repeat 1 Power Card with Energy cost of 1 or less.
			return RepeatPowerCard( ctx.Target, 2 );
		}

		[InnateOption("2 sun,3 earth,2 plant")]
		static public Task Option2( TargetSpiritCtx ctx ) {
			//  Instead, the Energy cost limit is 3 or less.
			return RepeatPowerCard( ctx.Target, 4);
		}

		[InnateOption("2 sun,4 earth,3 plant")]
		static public Task Option3( TargetSpiritCtx ctx ) {
			// Instead, the Energy cost limit is 6 or less.
			return RepeatPowerCard( ctx.Target, 6);
		}

		static Task RepeatPowerCard( Spirit spirit, int maxCost ) {
			spirit.AddActionFactory(new ReplayCardForCost(maxCost));
			return Task.CompletedTask;
		}

	}


}
