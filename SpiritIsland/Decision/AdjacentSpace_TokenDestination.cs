using System.Collections.Generic;

namespace SpiritIsland.Decision {

	public class AdjacentSpace_TokenDestination : AdjacentSpace {

		public AdjacentSpace_TokenDestination( Token token, Space source, IEnumerable<Space> destinationOptions, Present present )
			: base( "Push " + token.Summary + " to", source, AdjacentDirection.Outgoing, destinationOptions, present ) {
			Source = source;
			Token = token;
		}

		public Space Source { get; }
		public Token Token { get; }
	}

}