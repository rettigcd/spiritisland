using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class PushDahanDecision : TypedDecision<Space> {

		public PushDahanDecision( Space source, IEnumerable<Space> destinationOptions, Present present)
			: base( "Select destination for dahan", destinationOptions.ToArray(), present ) {
			Source = source;
		}

		public Space Source { get; }
	}


}