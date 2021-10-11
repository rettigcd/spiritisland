using System.Collections.Generic;

namespace SpiritIsland.Decision {

	// For Selecting Token from single space
	// !! This could be merged into SpaceTokens
	public class TokenOnSpace : TypedDecision<Token> {

		public TokenOnSpace( 
			string prompt, 
			Space space, IEnumerable<Token> options, 
			Present present 
		)
			: base( prompt, options, present ) 
		{ 
			Space = space;
		}
		public Space Space { get; }
	}

}