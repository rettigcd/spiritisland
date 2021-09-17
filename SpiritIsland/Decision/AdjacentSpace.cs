using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Decision {

	/// <summary>
	/// For when we are going to push/pull tokens between current space and an adjacent one.
	/// </summary>
	public class AdjacentSpace : TypedDecision<Space>, IPerformGatherOrPush {

		public AdjacentSpace( string prompt, Space original, GatherPush gatherPush, IEnumerable<Space> spaces, Present present = Present.IfMoreThan1 )
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

	public interface IPerformGatherOrPush {
		public GatherPush GatherPush { get; }
		public Space Original { get; }
		public Space[] Adjacent { get; }
	}

}