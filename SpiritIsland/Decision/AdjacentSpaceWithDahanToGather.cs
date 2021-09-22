using System.Collections.Generic;

namespace SpiritIsland.Decision {

	public class AdjacentSpaceWithDahanToGather : AdjacentSpace {

		public AdjacentSpaceWithDahanToGather( int remaining, Space to, IEnumerable<Space> spaces, Present present = Present.Always )
			: base( $"Gather Dahan ({remaining} remaining)", to, GatherPush.Gather, spaces, present ) {
		}

	}

}