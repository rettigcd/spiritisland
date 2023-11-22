namespace SpiritIsland;

public interface IPresenceTrack {

	// Read
	IEnumerable<Track> RevealOptions { get; }
	IEnumerable<Track> ReturnableOptions { get; }
	IEnumerable<Track> Revealed { get; }
	IReadOnlyCollection<Track> Slots { get; }

	// Modify/Act
	void AddElementsTo( CountDictionary<Element> elements );
	Task<bool> Reveal( Track track );
	bool Return( Track track );
	AsyncEvent<TrackRevealedArgs> TrackRevealed { get; }


	// Save/Load
	void LoadFrom( IMemento<IPresenceTrack> memento );
	IMemento<IPresenceTrack> SaveToMemento();
}

public class TrackRevealedArgs {
	public TrackRevealedArgs( Track track ) { Track = track; }
	public Track Track { get; }

}
