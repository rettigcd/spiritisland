using System.Collections.Generic;

namespace SpiritIsland {

	public interface IPresenceTrack {

		// Read
		IEnumerable<Track> RemovableOptions { get; }
		IEnumerable<Track> ReturnableOptions { get; }
		IEnumerable<Track> Revealed { get; }
		IReadOnlyCollection<Track> Slots { get; }

		// Modify/Act
		void AddElements( ElementCounts elements );
		bool Remove( Track track );
		bool Return( Track track );

		// Save/Load
		void LoadFrom( IMemento<IPresenceTrack> memento );
		IMemento<IPresenceTrack> SaveToMemento();
	}

}