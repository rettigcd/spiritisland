using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class GatherTokensFromDecision : SelectAdjacentDecision {

		public GatherTokensFromDecision( int remaining, TokenGroup[] ofType, Space to, IEnumerable<Space> spaces, Present present = Present.IfMoreThan1 )
			: base( $"Gather {ofType.Select( it => it.Label ).Join( "/" )} ({remaining} remaining)", to, GatherPush.Gather, spaces, present ) {
		}

	}

}