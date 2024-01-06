namespace SpiritIsland;

public class PresenceTrack : IPresenceTrack {

	#region constructor

	public PresenceTrack( int revealedCount, params Track[] slots ) {
		_slots = slots;
		_revealedCount = revealedCount;

		void Slot_Revealed( Track track ) {
			if(_revealedCount == _slots.Length) throw new InvalidOperationException("all slots revealed");
			if(_slots[_revealedCount] != track) throw new InvalidOperationException("track/slot is not the next to be revealed");
			++_revealedCount;
			TrackRevealed?.Invoke( new TrackRevealedArgs( track ) );
		}
		foreach(Track slot in slots)
			slot.Revealed += Slot_Revealed;
	}

	public PresenceTrack( params Track[] slots ):this(1,slots) { }

	#endregion

	public virtual IEnumerable<Track> RevealOptions {
		get {
			if(_revealedCount < _slots.Length)
				yield return _slots[_revealedCount];
		}
	}

	public virtual IEnumerable<Track> ReturnableOptions {
		get {
			if(_revealedCount > 1)
				yield return _slots[_revealedCount - 1];
		}
	}

	public IReadOnlyCollection<Track> Slots => _slots;

	public IEnumerable<Track> Revealed => _slots.Take( _revealedCount );

	public bool Reveal( Track track ) {
		if(_revealedCount == _slots.Length || _slots[_revealedCount] != track) return false;
		++_revealedCount;
		TrackRevealed?.Invoke( new TrackRevealedArgs( track ) );
		return true;
	}

	public event Action<TrackRevealedArgs> TrackRevealed;

	public virtual bool Return( Track track ) {
		if(_slots[_revealedCount - 1] != track) return false;
		--_revealedCount;
		return true;
	}

	public void AddElementsTo( CountDictionary<Element> elements ) {
		foreach(var r in Revealed)
			r.AddElementsTo( elements );
	}

	#region Memento

	// Revealed Count + Placed.
	object IHaveMemento.Memento {
		get => new Memento( this );
		set => ((Memento)value).Restore( this );
	}

	protected class Memento : IMemento<IPresenceTrack> {
		public Memento( PresenceTrack src ) { revealed = src._revealedCount; }
		public void Restore( PresenceTrack src ) { src._revealedCount = revealed; }
		readonly int revealed;
	}

	#endregion

	protected Track[] _slots;
	int _revealedCount;

}