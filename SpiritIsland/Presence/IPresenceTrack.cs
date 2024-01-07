namespace SpiritIsland;

public interface IPresenceTrack: IHaveMemento {

	// Read
	IEnumerable<Track> RevealOptions { get; }
	IEnumerable<Track> ReturnableOptions { get; }
	IEnumerable<Track> Revealed { get; }
	IReadOnlyCollection<Track> Slots { get; }

	// Modify/Act
	void AddElementsTo( CountDictionary<Element> elements );
	bool Reveal( Track track );
	bool Return( Track track );
	event Action<TrackRevealedArgs> TrackRevealed;

}

public class TrackRevealedArgs {
	public TrackRevealedArgs( Track track ) { Track = track; }
	public Track Track { get; }

}
