using System.Collections.Generic;

namespace SpiritIsland.Decision {

	public class TargetSpirit : TypedDecision<Spirit> {

		public TargetSpirit( string powerName, IEnumerable<Spirit> spirits, Present present = Present.Always )
			: base( powerName+": Target Spirit", spirits, present ) {
		}

	}
}
