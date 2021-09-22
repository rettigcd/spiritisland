using System.Collections.Generic;

namespace SpiritIsland.Decision {

	class TargetSpirit : TypedDecision<Spirit> {

		public TargetSpirit( IEnumerable<Spirit> spirits )
			: base( "Select Spirit to target", spirits, Present.Always ) {
		}

	}
}
