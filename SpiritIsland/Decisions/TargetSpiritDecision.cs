using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	class TargetSpiritDecision : TypedDecision<Spirit> {
		public TargetSpiritDecision( IEnumerable<Spirit> spirits )
			: base( "Select Spirit to target", spirits, Present.IfMoreThan1 ) {
		}
	}
}
