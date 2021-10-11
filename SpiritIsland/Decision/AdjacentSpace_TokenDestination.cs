using System.Collections.Generic;

namespace SpiritIsland.Decision {

	public class AdjacentSpace_TokenDestination : AdjacentSpace {

		public AdjacentSpace_TokenDestination( Token specific, Space source, IEnumerable<Space> destinationOptions, Present present )
			: base( "Push " + specific.Summary + " to", source, AdjacentDirection.Outgoing, destinationOptions, present ) {
			Source = source;
			Invader = specific;
		}

		public Space Source { get; }
		public Token Invader { get; }
	}

}