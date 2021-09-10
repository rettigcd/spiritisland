using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Decision {

	public class InvaderToDamage : TokenOnSpace {

		public InvaderToDamage(int maxDamage, Space space, IEnumerable<Token> options, Present present )
			: base( $"Apply damage({maxDamage}) to:", space, options.ToArray(), present ) { }

	}

}