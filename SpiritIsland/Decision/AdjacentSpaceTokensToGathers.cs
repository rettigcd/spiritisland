using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Decision {
	public class AdjacentSpaceTokensToGathers : TypedDecision<SpaceToken>, IsGatherOrPush  {

		public AdjacentSpaceTokensToGathers(
			int remaining,
			Space to,
			IEnumerable<SpaceToken> tokens,
			Present present = Present.IfMoreThan1
		) : base(
			present == Present.Done 
				? $"Gather up to {remaining}"
				: $"Gather {remaining}",
			tokens,
			present
		) {
			Original = to;
			Adjacent = tokens.Select(s=>s.Space).Distinct().ToArray();
		}

		public GatherPush GatherPush => GatherPush.Gather;

		public Space Original { get; }

		public Space[] Adjacent { get; }
	}


}