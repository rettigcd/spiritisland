using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class PushTokenDecision : SelectAdjacentDecision {

		public PushTokenDecision( Token specific, Space source, IEnumerable<Space> destinationOptions, Present present )
			: base( "Push " + specific.Summary + " to", source, GatherPush.Push, destinationOptions, present ) {
			Source = source;
			Invader = specific;
		}

		public Space Source { get; }
		public Token Invader { get; }
	}

}