using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	// This isn't really an Action, it is more of a special ability.
	// User shouldn't have to click-on-it to activate it.


	[InnatePower( "Let Them Break Themselves Against the Stone Bulwark of Will" ), Fast, Yourself]
	class HoldTheIslandFastWithABulwarkOfWill {

		[InnateOption("2 earth","When blight is added to one of your lands, you may pay 2 Energy per blight to take it from the box instead of the Blight Card.")]
		static public async Task Option1(TargetSpaceCtx ctx ) {
		}

		[InnateOption("4 earth","The cost is 1 Energy instead of 2")]
		static public async Task Option2(TargetSpaceCtx ctx ) {
		}

		[InnateOption("6 earth,1 plant","When an Event or Blight card directly destorys presence (yours or others'), you may prevent any number of presence from being destroyed by paying 1 Energy each.")]
		static public async Task Option3(TargetSpaceCtx ctx ) {
		}

	}

}
