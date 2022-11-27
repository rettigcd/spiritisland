namespace SpiritIsland.PromoPack1;

public class FinderPresence : SpiritPresence {

	#region Custom Tracks

	static Track OneMoon => new Track( "1,Moon", Element.Moon ) {
		Energy = 1,
		Icon = new IconDescriptor {
			Text = "1",
			BackgroundImg = Img.Coin,
			Sub = new IconDescriptor { BackgroundImg = Img.Token_Moon }
		}
	};
	static Track TwoWater => new Track( "2,Water", Element.Water ) {
		Energy = 2,
		Icon = new IconDescriptor {
			Text = "2",
			BackgroundImg = Img.Coin,
			Sub = new IconDescriptor { BackgroundImg = Img.Token_Water }
		}
	};

	static Track Move1Air => new Track( "Moveonepresence.png", Element.Air ) {
		Action = new MovePresence( 1 ),
		Icon = new IconDescriptor {
			BackgroundImg = Img.MovePresence,
			Sub = new IconDescriptor { BackgroundImg = Img.Token_Air }
		}
	};

	static Track Plus2 => new Track( "+2" ) {
		Action = new GainEnergy( 2 ),
		Icon = new IconDescriptor {
			Text = "+2",
			BackgroundImg = Img.Coin,
		}
	};

	static Track PlusOneAny => new Track( "+1,Any", Element.Any ) {
		Action = new GainEnergy( 1 ),
		Icon = new IconDescriptor {
			Text = "+1",
			BackgroundImg = Img.Coin,
			Sub = new IconDescriptor { BackgroundImg = Img.Token_Any }
		}
	};

	static Track ExtraCardPlay => new Track( "+1 Card PLay" ) {
		Action = new PlayExtraCardThisTurn( 1 ),
		Icon = new IconDescriptor { BackgroundImg = Img.CardPlayPlusN, Text = "+1" }
	};

	static Track OneCoinExtendRange => new Track( "+1,+1 range" ) {
		Action = new CompoundActionFactory( new GainEnergy( 1 ), new ExtendRange( 1 ) ),
		Icon = new IconDescriptor {
			BackgroundImg = Img.Coin,
			Text = "+1",
			BigSub = new IconDescriptor { BackgroundImg = Img.PlusOneRange }
		}
	};

	#endregion


	public FinderPresence():base(
		new FinderTrack( Track.Energy0, Track.SunEnergy, OneMoon, TwoWater, Move1Air, Plus2, PlusOneAny ),
		new FinderTrack( Track.Card1, Track.EarthEnergy, ExtraCardPlay, ExtraCardPlay, OneCoinExtendRange, Track.Push1TownCity )
	) {
		var energy = ((FinderTrack)Energy).LinkedSlots;
		FinderTrack.LinkedSlot energy0    = energy[0];
		FinderTrack.LinkedSlot sunEnergy  = energy[1];
		FinderTrack.LinkedSlot oneMoon    = energy[2];
		FinderTrack.LinkedSlot twoWater   = energy[3];
		FinderTrack.LinkedSlot move1Air   = energy[4];
		FinderTrack.LinkedSlot plus2      = energy[5];
		FinderTrack.LinkedSlot plusOneAny = energy[6];

		var card = ((FinderTrack)CardPlays).LinkedSlots;
		FinderTrack.LinkedSlot card1              = card[0];
		FinderTrack.LinkedSlot earthEnergy        = card[1];
		FinderTrack.LinkedSlot extraCardPlay1     = card[2];
		FinderTrack.LinkedSlot extraCardPlay2     = card[3];
		FinderTrack.LinkedSlot oneCoinExtendRange = card[4];
		FinderTrack.LinkedSlot push1TownCity      = card[5];

		// Top row links - 4 links
		sunEnergy.TwoWay( energy0, twoWater );
		plus2.TwoWay( twoWater, plusOneAny );
		// Bottom row links - 4 links
		earthEnergy.TwoWay( card1, extraCardPlay1 );
		extraCardPlay2.TwoWay( extraCardPlay1, push1TownCity );
		// Middle Row - link up and down and across - 10 links
		oneMoon.TwoWay( sunEnergy, twoWater, move1Air, earthEnergy );
		move1Air.TwoWay( plus2, extraCardPlay2 );
		oneCoinExtendRange.TwoWay( plus2, plusOneAny, extraCardPlay2, push1TownCity );

		energy0.Reveal();
		card1.Reveal();

		base.InitEnergyAndCardPlays(); // need to rescan because spaces weren't revealed yet.
	}

}

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

	public async Task<bool> Reveal( Track track, GameState gameState ) {
		if(!_lookup.ContainsKey( track )) return false;
		_lookup[track].Reveal();
		await TrackRevealed?.InvokeAsync(new TrackRevealedArgs( track, gameState));
		return true;
	}

	#region Load / Save

	public void LoadFrom( IMemento<IPresenceTrack> memento ) {
	}

	public IMemento<IPresenceTrack> SaveToMemento() {
		return null; // !!!
	}

	#endregion

	Dictionary<Track, LinkedSlot> _lookup;

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
}