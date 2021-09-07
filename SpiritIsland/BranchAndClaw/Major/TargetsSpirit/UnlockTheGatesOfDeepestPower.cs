using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class UnlockTheGatesOfDeepestPower {

		[MajorCard( "Unlock the Gates of Deepenst Power", 4, Speed.Fast, Element.Sun,Element.Moon,Element.Fire,Element.Air,Element.Water,Element.Earth,Element.Plant,Element.Animal )]
		[TargetSpirit]
		static public Task ActAsync( TargetSpiritCtx ctx ) {
			// target psirit gains a major power by drawing 2 and keeping 1, without having to forget another power card
			// if 2 of each element,
			// target psirit may now play the major power they keep by paying half its cost (round up)  OR by forgetting it at the end of turn.  It gains all elmemental thresholds.
			return Task.CompletedTask;
		}

	}

}
