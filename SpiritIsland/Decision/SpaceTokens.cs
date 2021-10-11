using System.Collections.Generic;

namespace SpiritIsland.Decision {

	// For Selecting Token from multiple spaces
	public class SpaceTokens : TypedDecision<SpaceToken> {

		public SpaceTokens(
			string prompt,
			IEnumerable<SpaceToken>tokens,
			Present present
		)
			: base( prompt, tokens, present ) 
		{
		}

	}

}