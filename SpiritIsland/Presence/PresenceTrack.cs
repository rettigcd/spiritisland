using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class PresenceTrack : IPresenceTrack {

		#region constructor

		public PresenceTrack( params Track[] slots ) {
			this.slots = slots;
		}

		#endregion

		public virtual IEnumerable<Track> RevealOptions {
			get {
				if(revealedCount < slots.Length)
					yield return slots[revealedCount];
			}
		}

		public virtual IEnumerable<Track> ReturnableOptions {
			get {
				if(revealedCount > 1)
					yield return slots[revealedCount - 1];
			}
		}

		public IReadOnlyCollection<Track> Slots => slots; // !!! switch to something read-only

		public IEnumerable<Track> Revealed => slots.Take( revealedCount );

		public virtual async Task<bool> Reveal( Track track, GameState gameState ) {
			if(revealedCount == slots.Length || slots[revealedCount] != track) return false;
			++revealedCount;
			await TrackRevealed.InvokeAsync( gameState, track );
			return true;
		}

		public DualAsyncEvent<Track> TrackRevealed { get; } = new DualAsyncEvent<Track>();

		public virtual bool Return( Track track ) {
			if(slots[revealedCount - 1] != track) return false;
			--revealedCount;
			return true;
		}

		public void AddElements( ElementCounts elements ) {
			foreach(var r in Revealed)
				r.AddElement( elements );
		}

		#region Memento

		// Revealed Count + Placed.
		public IMemento<IPresenceTrack> SaveToMemento() => new Memento( this );
		public void LoadFrom( IMemento<IPresenceTrack> memento ) => ((Memento)memento).Restore( this );

		protected class Memento : IMemento<IPresenceTrack> {
			public Memento( PresenceTrack src ) { revealed = src.revealedCount; }
			public void Restore( PresenceTrack src ) { src.revealedCount = revealed; }
			readonly int revealed;
		}

		#endregion

		protected Track[] slots;
		int revealedCount = 1;

	}

}