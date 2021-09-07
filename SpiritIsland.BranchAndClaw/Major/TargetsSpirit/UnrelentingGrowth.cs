using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class UnrelentingGrowth {

		[MajorCard( "Unrelenting Growth", 4, Speed.Slow, Element.Sun, Element.Fire, Element.Water, Element.Plant )]
		[TargetSpirit]
		static public Task ActAsync( TargetSpiritCtx ctx ) {
			// target spirit adds 2 presence and 1 wilds to a land at range 1
			// if you have 3 sun, 3 plant
			// in that land add 1 additional wilds and remove 1 blight.  Target Spirit gains a power card.
			return Task.CompletedTask;
		}

	}

}
