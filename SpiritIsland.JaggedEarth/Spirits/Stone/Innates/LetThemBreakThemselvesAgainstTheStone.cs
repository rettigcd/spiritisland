using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {
	[InnatePower("Let Them Break Themselves Against the Stone"), Fast, FromPresence(0)]
	class LetThemBreakThemselvesAgainstTheStone {

		[InnateOption("5 earth","Also deal half of the Damage Invaders did to the land (rounding down)")]
		static public async Task Option1(TargetSpaceCtx ctx ) {
		}

		[InnateOption("7 earth,2 sun","Repeat this power")]
		static public async Task Option2(TargetSpaceCtx ctx ) {
		}


	}

}
