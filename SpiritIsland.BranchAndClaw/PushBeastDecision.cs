using System.Collections.Generic;

namespace SpiritIsland.BranchAndClaw {
	public class PushBeastDecision : Decision.AdjacentSpace {

		public PushBeastDecision( Space source, IEnumerable<Space> destinationOptions, Present present )
			: base( "Select destination for beasts", source, SpiritIsland.Decision.AdjacentDirection.Outgoing, destinationOptions, present ) {
			Source = source;
		}

		public Space Source { get; }
	}


}
