using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class PushInvaderDecision : SelectAdjacentDecision {

		public PushInvaderDecision( InvaderSpecific specific, Space source, IEnumerable<Space> destinationOptions, Present present )
			: base( "Push " + specific.Summary + " to", source, destinationOptions, present ) {
			Source = source;
			Invader = specific;
		}

		public Space Source { get; }
		public InvaderSpecific Invader { get; }
	}

}