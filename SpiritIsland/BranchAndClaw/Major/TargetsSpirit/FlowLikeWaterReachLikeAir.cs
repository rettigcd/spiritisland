using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw {

	public class FlowLikeWaterReachLikeAir {

		[MajorCard("Flow Like Water, Reach Like Air",2,Speed.Fast,Element.Air,Element.Water)]
		[TargetSpirit]
		static public Task ActAsync(TargetSpiritCtx ctx ) {
			// target spirit gets +2 range with all Powers.
			// Target spirit may push 1 of their presence to an adjacent land, bringing up to 2 explorers, 2 towns and 2 dahan along with it.
			// if you hvae 2 air, 2 water, the moved presence may also bring along up to 2 cities and up to 2 blight.
			return Task.CompletedTask;
		}

	}

}
