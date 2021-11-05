using System.Collections.Generic;

namespace SpiritIsland.Decision {

	class TargetSpirit : TypedDecision<Spirit> {

		public TargetSpirit( string powerName, IEnumerable<Spirit> spirits )
			: base( powerName+": Target Spirit", spirits, Present.Always ) {
		}

	}
}
