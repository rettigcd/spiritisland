using System.Collections.Generic;

namespace SpiritIsland {

	public class PushDahanDecision : SelectAdjacentDecision {

		public PushDahanDecision( Space source, IEnumerable<Space> destinationOptions, Present present)
			: base( "Select destination for dahan", source, GatherPush.Push, destinationOptions, present ) {
			Source = source;
		}

		public Space Source { get; }
	}


}