using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class SelectAdjacentDecision : TypedDecision<Space> {

		public SelectAdjacentDecision( string prompt, Space original, GatherPush gatherPush, IEnumerable<Space> spaces, Present present = Present.IfMoreThan1 )
			: base( prompt, spaces.OrderBy( x => x.Label ), present ) {
			Original = original;
			GatherPush = gatherPush;
			Adjacent = spaces.ToArray();
		}

		public Space[] Adjacent { get; }

		public GatherPush GatherPush { get; }

		public Space Original { get; }

	}

	public enum GatherPush { None, Gather, Push }

}