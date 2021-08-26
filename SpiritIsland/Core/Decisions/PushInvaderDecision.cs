using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class PushInvaderDecision : TypedDecision<Space> {

		public PushInvaderDecision( InvaderSpecific specific, Space source, IEnumerable<Space> destinationOptions, Present present )
			: base( "Push " + specific.Summary + " to", destinationOptions.ToArray(), present ) {
			Source = source;
			Invader = specific;
		}

		public Space Source { get; }
		public InvaderSpecific Invader { get; }
	}

}