using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class TargetSpaceDecision : TypedDecision<Space> {

		public TargetSpaceDecision( string prompt, IEnumerable<Space> spaces, Present present = Present.IfMoreThan1 )
			: base( prompt, spaces.OrderBy( x => x.Label ).ToArray(), present ) {
		}

	}

}