namespace SpiritIsland;

public interface IPresenceTrack {

	// Read
	IEnumerable<Track> RevealOptions { get; }
	IEnumerable<Track> ReturnableOptions { get; }
	IEnumerable<Track> Revealed { get; }
	IReadOnlyCollection<Track> Slots { get; }

	// Modify/Act
	void AddElements( ElementCounts elements );
	Task<bool> Reveal( Track track, GameState gameState );
	bool Return( Track track );
	AsyncEvent<TrackRevealedArgs> TrackRevealed { get; }


	// Save/Load
	void LoadFrom( IMemento<IPresenceTrack> memento );
	IMemento<IPresenceTrack> SaveToMemento();
}

public class TrackRevealedArgs {

	public TrackRevealedArgs( Track track, GameState gameState ) {
		Track = track;
		GameState = gameState ?? throw new ArgumentNullException( nameof( gameState ) );
	}

	public Track Track { get; }
	public GameState GameState { get;}
}
