namespace SpiritIsland;

public class PresenceTrack : IPresenceTrack {

	#region constructor

	public PresenceTrack( int revealedCount, params Track[] slots ) {
		_slots = slots;
		_revealedCount = revealedCount;

		foreach(Track slot in slots)
			slot.SourcedTokenAsync += Slot_SourcedTokenAsync;

		async Task Slot_SourcedTokenAsync( Track track ) {
			if(_revealedCount == _slots.Length) throw new InvalidOperationException("all slots revealed");
			if(_slots[_revealedCount] != track) throw new InvalidOperationException("track/slot is not the next to be revealed");
			await RevealAsync( track );
		}
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

	public async Task<bool> RevealAsync( Track track ) {
		if(_revealedCount == _slots.Length || _slots[_revealedCount] != track) return false;
		++_revealedCount;
		await OnTrackRevealedAsync( track );
		return true;
	}

	async Task OnTrackRevealedAsync( Track track ){
		if(TrackRevealedAsync is not null)
			await TrackRevealedAsync( new TrackRevealedArgs( track ));
	}

	public event Func<TrackRevealedArgs,Task>? TrackRevealedAsync;

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

	protected class Memento( PresenceTrack _src ) {
		public void Restore( PresenceTrack src ) { src._revealedCount = revealed; }
		readonly int revealed = _src._revealedCount;
	}

	#endregion

	protected Track[] _slots;
	int _revealedCount;

}