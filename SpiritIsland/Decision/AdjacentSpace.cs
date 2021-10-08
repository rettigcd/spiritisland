using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Decision {

	/// <summary>
	/// For when we are going to push/pull tokens between current space and an adjacent one.
	/// </summary>
	public class AdjacentSpace : TypedDecision<Space>, IAdjacentDecision {

		public AdjacentSpace( string prompt, Space original, AdjacentDirection gatherPush, IEnumerable<Space> spaces, Present present = Present.Always )
			: base( prompt, spaces.OrderBy( x => x.Label ), present ) {
			Original = original;
			Direction = gatherPush;
			Adjacent = spaces.ToArray();
		}

		public Space[] Adjacent { get; }

		public AdjacentDirection Direction { get; }

		public Space Original { get; }

	}

	public enum AdjacentDirection { None, Incoming, Outgoing }

	public interface IAdjacentDecision {
		public AdjacentDirection Direction { get; }
		public Space Original { get; }
		public Space[] Adjacent { get; }
	}

}