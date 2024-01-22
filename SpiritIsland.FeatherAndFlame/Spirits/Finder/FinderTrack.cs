namespace SpiritIsland.FeatherAndFlame;

public partial class FinderTrack : IPresenceTrack {

	public FinderTrack( params Track[] orderedSlots ) {
		Slots = Array.AsReadOnly( orderedSlots ); // Ordered
		_lookup = orderedSlots.ToDictionary(
			s=>s,
			s=> new LinkedSlot{ State = SlotState.Hidden_Not_Revealable } 
		);
		foreach(Track slot in Slots)
			slot.SourcedTokenAsync += (track) => RevealAsync(track);
	}

	public IReadOnlyCollection<Track> Slots { get; }
	public IEnumerable<Track> Revealed => IsOneOf(SlotState.Revealed_Not_Hideable,SlotState.Revealed_But_Hidable);

	public IEnumerable<Track> RevealOptions => IsOneOf(SlotState.Hidden_But_Revealable);

	public IEnumerable<Track> ReturnableOptions => IsOneOf(SlotState.Revealed_But_Hidable);

	public LinkedSlot[] LinkedSlots => Slots.Select(x=>_lookup[x]).ToArray();

	IEnumerable<Track> IsOneOf(params SlotState[] states) => _lookup
		.Where( pair => pair.Value.State.IsOneOf(states))
		.Select( pair => pair.Key );

	// ============================

	public event Func<TrackRevealedArgs,Task> TrackRevealedAsync;

	public void AddElementsTo( CountDictionary<Element> elements ) {
		foreach(Track r in Revealed)
			r.AddElementsTo( elements );
	}

	// ============================

	public bool Return( Track track ) {
		bool found = _lookup.TryGetValue( track, out LinkedSlot linkedSlot );
		if(found) linkedSlot.Hide();
		return found;
	}

	public async Task<bool> RevealAsync( Track track ) {
		if(!_lookup.TryGetValue( track, out LinkedSlot value )) return false;
		await value.RevealAsync();
		if(TrackRevealedAsync is not null)
			await TrackRevealedAsync(new TrackRevealedArgs( track ));
		return true;
	}

	#region Load / Save

	object IHaveMemento.Memento {
		get {
			return new MyMemento { TrackStates = _lookup.ToDictionary( p => p.Key, p => p.Value.State ) };
		}
		set {
			var src = (MyMemento)value;
			foreach(var p in src.TrackStates)
				_lookup[p.Key].State = p.Value;
		}
	}

	class MyMemento {
		public Dictionary<Track, SlotState> TrackStates;
	};

#endregion

	public enum SlotState {
		Hidden_Not_Revealable,
		Hidden_But_Revealable,
		Revealed_But_Hidable,
		Revealed_Not_Hideable
	}

	// Holds linkage and state
	readonly Dictionary<Track, LinkedSlot> _lookup;

}