﻿using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Decision {
	public class AdjacentSpaceTokensToGathers : TypedDecision<SpaceToken>, IAdjacentDecision  {

		public AdjacentSpaceTokensToGathers(
			int remaining,
			Space to,
			IEnumerable<SpaceToken> tokens,
			Present present = Present.Always
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

		public AdjacentDirection Direction => AdjacentDirection.Incoming;

		public Space Original { get; }

		public Space[] Adjacent { get; }
	}


}