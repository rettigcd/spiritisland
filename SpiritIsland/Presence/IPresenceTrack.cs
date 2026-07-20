namespace SpiritIsland;

public interface IPresenceTrack: IHaveMemento {

	// Read
	IEnumerable<Track> RevealOptions { get; }
	IEnumerable<Track> ReturnableOptions { get; }
	IEnumerable<Track> Revealed { get; }
	IReadOnlyCollection<Track> Slots { get; }

	// Modify/Act
	void AddElementsTo( CountDictionary<Element> elements );
	Task<bool> RevealAsync( Track track );
	bool Return( Track track );
	event Func<TrackRevealedArgs,Task> TrackRevealedAsync;

	/// <summary>
	/// Only 3 implementers exist solution-wide (PresenceTrack, CompoundPresenceTrack, FinderTrack) - a
	/// plain interface member is enough, no registry needed (same reasoning as
	/// ITargetingSourceStrategy.ToJson, minus the ISerializationContext - no Spirit/SpaceSpec references
	/// are involved here). Closes the gap docs/GameSerialization-Roadmap.md's Spirit-core-state section
	/// used to flag: SpiritPresence.ToJson/RestoreFromJson used to throw for anything but a plain
	/// PresenceTrack. Restores onto an already-constructed track (same "existing target" shape as
	/// PresenceTrack.SetRevealedCount already used), not a freshly-built one - track *structure* (which
	/// slots exist) is spirit-type data, identical every time the same concrete Spirit is reconstructed.
	/// </summary>
	JsonNode ToJson();
	void RestoreFromJson( JsonNode json );

}

public class TrackRevealedArgs( Track track ) {
	public Track Track { get; } = track;
}
