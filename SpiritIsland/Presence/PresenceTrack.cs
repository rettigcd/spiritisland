using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	public class PresenceTrack {
		public PresenceTrack( params Track[] slots ) {
			this.slots = slots;
		}

		public Track[] slots;
		public IEnumerable<Track> Revealed => slots.Take(RevealedCount);

		public Track Next => slots[RevealedCount];
		public bool HasMore => RevealedCount < slots.Length;
		public int TotalCount => slots.Length;
		public int RevealedCount { get; set; } = 1;
	}

}