using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Decision {

	public class TargetSpace : TypedDecision<Space> {

		public TargetSpace( string prompt, IEnumerable<Space> spaces, Present present = Present.Always )
			: base( prompt, spaces.OrderBy( x => x.Label ), present ) {
		}

	}

}