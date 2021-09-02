using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {
	public class SelectInvaderToDamage : InvadersOnSpaceDecision {
		public SelectInvaderToDamage(int maxDamage, Space space, IEnumerable<Token> options, Present present )
			: base( $"Apply damage({maxDamage}) to:", space, options.ToArray(), present ) { }
	}


}