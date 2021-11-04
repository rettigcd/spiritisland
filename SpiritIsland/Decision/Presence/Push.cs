using System.Collections.Generic;

namespace SpiritIsland.Decision.Presence {
	/// This does not inherit from Deployed 
	/// because we are not selecting spaces where presence is deployed
	/// but the Spaces around it.
	/// </remarks>
	public class Push : AdjacentSpace {

		public Push(string prompt, Space source, IEnumerable<Space> destinationOptions ) 
			:base(prompt, source, AdjacentDirection.Outgoing, destinationOptions)
		{}

	}


}
