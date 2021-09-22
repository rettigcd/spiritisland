using System.Collections.Generic;

namespace SpiritIsland.BranchAndClaw {
	public class GatherBeastsFromDecision : Decision.AdjacentSpace {
		public GatherBeastsFromDecision( int remaining, Space to, IEnumerable<Space> spaces, Present present = Present.Always )
			: base( $"Gather Beasts ({remaining} remaining)", to, SpiritIsland.Decision.GatherPush.Gather, spaces, present ) {
		}
	}


}
