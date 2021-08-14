using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {
	public class MyPresence {

		public MyPresence(PresenceTrack energy, PresenceTrack cardPlays){
			Energy = energy;
			CardPlays = cardPlays;
		}

		public void Place(Space space) => Placed.Add(space);
		public void Place( IEnumerable<Space> spaces ) => Placed.AddRange( spaces );
		public IEnumerable<Space> Spaces => Placed.Distinct();

		public int On(Space space) => Placed.Count(p=>p==space);
		public bool IsOn(Space space) => Placed.Contains(space);

		public void Destroy(Space space) { Placed.Remove(space); ++destroyed; } 
		int destroyed = 0;

		public void Move(Space from, Space to ) {
			Placed.Remove( from );
			Placed.Add( to );
		}

		public PresenceTrack Energy { get; }
		public PresenceTrack CardPlays { get; }

		public readonly List<Space> Placed = new List<Space>();

	}

}