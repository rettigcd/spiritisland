using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class SelectAdjacentDecision : TypedDecision<Space> {

		public SelectAdjacentDecision( string prompt, Space original, IEnumerable<Space> spaces, Present present = Present.IfMoreThan1 )
			: base( prompt, spaces.OrderBy( x => x.Label ).ToArray(), present ) {
			Original = original;
		}

		public Space Original { get; }

	}

}