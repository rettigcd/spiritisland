namespace SpiritIsland.FeatherAndFlame;

public class FinderTrack : IPresenceTrack {

	public FinderTrack( params Track[] orderedSlots ) {
		Slots = Array.AsReadOnly( orderedSlots ); // Ordered
		_lookup = orderedSlots.ToDictionary(s=>s,s=> new LinkedSlot{ State = SlotState.Hidden_Not_Revealable } );
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

	public AsyncEvent<TrackRevealedArgs> TrackRevealed { get; } = new AsyncEvent<TrackRevealedArgs>();

	public void AddElementsTo( ElementCounts elements ) {
		foreach(var r in Revealed)
			r.AddElement( elements );
	}

	// ============================

	public bool Return( Track track ) {
		if(!_lookup.ContainsKey(track)) return false;
		_lookup[track].Hide();
		return true;
	}

	public async Task<bool> Reveal( Track track ) {
		if(!_lookup.ContainsKey( track )) return false;
		_lookup[track].Reveal();
		await TrackRevealed?.InvokeAsync(new TrackRevealedArgs( track ));
		return true;
	}

	#region Load / Save

	public void LoadFrom( IMemento<IPresenceTrack> memento ) {
		var src = (MyMemento)memento;
		foreach(var p in src.TrackStates)
			_lookup[p.Key].State = p.Value;
	}

	public IMemento<IPresenceTrack> SaveToMemento() {
		return new MyMemento {
			TrackStates = _lookup.ToDictionary( p => p.Key, p => p.Value.State )
		};
	}

	class MyMemento : IMemento<IPresenceTrack> {
		public Dictionary<Track, SlotState> TrackStates;
	};

	#endregion

	public class LinkedSlot {
		public SlotState State { get; set; }

		public void Hide() {
			// Assert: is hidable
			State = SlotState.Hidden_But_Revealable;
			TellNextToRecheckForPreviousRevealed();
			TellPreviousTheyHaveANextHidden();
		}

		public void Reveal() {
			// Assert: is revealable (except for setup)
			State = SlotState.Revealed_But_Hidable;
			TellPreviousToRecheckForNextHidden();
			TellNextTheyHaveAPreviousRevealed();
		}

		#region tell prev/next that you changed

		void TellNextToRecheckForPreviousRevealed() {
			foreach(LinkedSlot item in Next)
				item.RecheckForPreviousRevealed(); // it was hidden!
		}

		void TellPreviousToRecheckForNextHidden() {
			foreach(LinkedSlot item in Previous)
				item.RecheckForNextHiddenSlot(); // it was revealed!
		}
		void TellNextTheyHaveAPreviousRevealed() {
			foreach(LinkedSlot next in Next)
				next.HasRevealedPreviousSlot();
		}

		void TellPreviousTheyHaveANextHidden() {
			foreach(LinkedSlot prev in Previous)
				prev.HasHiddenNextSlot();
		}

		#endregion

		#region change self state

		void RecheckForPreviousRevealed() {
			if( State == SlotState.Hidden_But_Revealable	// was Revealable
				&& !Previous.Any( s => s.State == SlotState.Revealed_But_Hidable ) // but no longer has an upstream revealed slot
			) 
				State = SlotState.Hidden_Not_Revealable;
		}

		void RecheckForNextHiddenSlot() {
			
			if( State == SlotState.Revealed_But_Hidable    // was Hideable
				&& !Next.Any( s => s.State == SlotState.Hidden_But_Revealable )  // no no longer  has any downstream hidden slot
			)
				State = SlotState.Revealed_Not_Hideable;
		}

		void HasHiddenNextSlot() {
			if(State == SlotState.Revealed_Not_Hideable)
				State = SlotState.Revealed_But_Hidable;
		}

		void HasRevealedPreviousSlot() {
			if(State == SlotState.Hidden_Not_Revealable)
				State = SlotState.Hidden_But_Revealable;
		}

		#endregion

		public void TwoWay(params LinkedSlot[] others ) {
			foreach(var other in others) {
				this.FlowsTo( other );
				other.FlowsTo( this );
			}
		}

		public void FlowsTo( LinkedSlot next ) {
			Next.AddLast( next );
			next.Previous.AddLast( this );
		}

		public LinkedList<LinkedSlot> Next { get; set; } = new LinkedList<LinkedSlot>();
		public LinkedList<LinkedSlot> Previous { get; set; } = new LinkedList<LinkedSlot>();
	}

	public enum SlotState {
		Hidden_Not_Revealable,
		Hidden_But_Revealable,
		Revealed_But_Hidable,
		Revealed_Not_Hideable
	}

	// Holds linkage and state
	readonly Dictionary<Track, LinkedSlot> _lookup;

}