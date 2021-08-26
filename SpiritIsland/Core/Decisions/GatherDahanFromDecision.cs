using System.Collections.Generic;

namespace SpiritIsland {
	public class GatherDahanFromDecision : SelectAdjacentDecision {
		public GatherDahanFromDecision( int remaining, Space to, IEnumerable<Space> spaces, Present present = Present.IfMoreThan1 )
			: base( $"Gather Dahan ({remaining} remaining)", to, GatherPush.Gather, spaces, present ) {
		}
	}

}