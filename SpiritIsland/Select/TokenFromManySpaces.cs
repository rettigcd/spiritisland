using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Select {

	// For Selecting Token from multiple spaces
	public class TokenFromManySpaces : TypedDecision<SpaceToken>, IHaveAdjacentInfo {

		static public TokenFromManySpaces ToGather(
			int remaining,
			SpiritIsland.Space to,
			IEnumerable<SpaceToken> tokens,
			Present present = Present.Always
		) => new TokenFromManySpaces(
			present == Present.Done 
				? $"Gather up to {remaining}"
				: $"Gather {remaining}",
			tokens,
			present
		) {
			AdjacentInfo = new AdjacentInfo {
				Original = to,
				Adjacent = tokens.Select(s=>s.Space).Distinct().ToArray(),
				Direction = AdjacentDirection.Incoming
			}
		};

		public TokenFromManySpaces(
			string prompt,
			IEnumerable<SpaceToken>tokens,
			Present present
		)
			: base( prompt, tokens, present ) 
		{}

		public AdjacentInfo AdjacentInfo { get; set; }

	}

}