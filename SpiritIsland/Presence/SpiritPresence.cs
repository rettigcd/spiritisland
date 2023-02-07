namespace SpiritIsland;

public class SpiritPresence : IKnowSpiritLocations {

	#region constructors

	public SpiritPresence( IPresenceTrack energy, IPresenceTrack cardPlays ) {
		Energy = energy;
		CardPlays = cardPlays;

		Energy.TrackRevealed.Add( OnRevealed );
		CardPlays.TrackRevealed.Add( OnRevealed );

		InitEnergyAndCardPlays();

		Token = new SpiritPresenceToken();
	}

	protected void InitEnergyAndCardPlays() {
		foreach(Track r in Energy.Revealed) CheckEnergyAndCardPlays( r );
		foreach(Track r in CardPlays.Revealed) CheckEnergyAndCardPlays( r );
	}

	public virtual void SetSpirit( Spirit spirit ) {
		if(Self != null) throw new InvalidOperationException();
		Self = spirit;
	}

	protected Spirit Self { get; set; }

	#endregion

	#region Tracks / Board

	public virtual IEnumerable<Track> RevealOptions()
		=> Energy.RevealOptions.Union( CardPlays.RevealOptions );

	public IEnumerable<Track> CoverOptions
		=> Energy.ReturnableOptions.Union( CardPlays.ReturnableOptions );

	public int EnergyPerTurn { get; private set; }

	// ! These 2 tracks are only public so they can be accessed from Tests.
	// ! Not accessed from production code
	public IPresenceTrack Energy { get; }
	public IPresenceTrack CardPlays { get; }

	public int Destroyed {
		get => Token.Destroyed;
		set => Token.Destroyed = value;
	}

	/// <summary> Removes a Destroyed Presence from the game. </summary>
	public void RemoveDestroyed( int count ) {
		if(count > Destroyed) throw new ArgumentOutOfRangeException( nameof( count ) );
		Destroyed -= count;
	}

	#endregion

	#region Readonly 

	public IEnumerable<IActionFactory> RevealedActions
		=> CardPlays.Revealed
			.Union( Energy.Revealed )
			.Select( x => x.Action )
			.Where( x => x != null );

	public int CardPlayCount { get; private set; }

	public ElementCounts TrackElements {
		get {
			var elements = new ElementCounts();
			Energy.AddElementsTo( elements );
			CardPlays.AddElementsTo( elements );
			return elements;
		}
	}

	#endregion

	#region nice Predicate methods 

	/// <summary>
	/// Specifies if the the given space is valid.
	/// </summary>
	public virtual bool CanBePlacedOn( SpaceState spaceState ) => UnitOfWork.Current.TerrainMapper.IsInPlay( spaceState );
	public bool IsOn( SpaceState spaceState ) => 0 < spaceState[Token];
	public virtual bool IsSacredSite( SpaceState space ) => 2 <= space[Token];
	public int CountOn( SpaceState spaceState ) => spaceState[Token];

	#endregion

	#region Game-Play things you can do with presence

	public async Task Place( IOption from, Space to ) {
		await TakeFrom( from );
		await to.Tokens.Add( Token, 1 );
	}

	public async Task TakeFrom( IOption from ) {
		if(from is Track track)
			await RevealTrack( track );
		else if(from is Space space)
			await TakeFromSpace( space );
	}

	async Task TakeFromSpace( Space space ) {
		SpaceState fromSpace = space.Tokens;
		if(IsOn( fromSpace ))
			await fromSpace.Remove( Token, 1, RemoveReason.MovedFrom );
		else
			throw new ArgumentException( "Can't pull from island space:" + space.ToString() );
	}

	public Task ReturnDestroyedToTrack( Track dst ) {

		// src / from
		if(Destroyed <= 0) throw new InvalidOperationException( "There is no Destroyed presence to return." );
		--Destroyed;

		// !!! This should reduce card plays or energy but I can't find that anywhere.

		// To
		return Return( dst );
	}

	public Task Return( Track dst ) {
		return (Energy.Return( dst ) || CardPlays.Return( dst ))
			? Task.CompletedTask
			: throw new ArgumentException( "Unable to find location to restore presence" );
	}

	public bool HasMovableTokens( SpaceState spaceState ) => CanMove && IsOn( spaceState );

	public bool CanMove { get; set; } = true; // Spirit effect - Settle Into Hunting Grounds

	protected virtual async Task RevealTrack( Track track ) {
		if(track == Track.Destroyed && Destroyed > 0)
			--Destroyed;
		else {
			bool energyRevealed = await Energy.Reveal( track );
			if(!energyRevealed) {
				bool cardRevealed = await CardPlays.Reveal( track );
				if(!cardRevealed)
					throw new ArgumentException( "Can't pull from track:" + track.ToString() );
			}
		}
	}

	Task OnRevealed( TrackRevealedArgs args ) {
		CheckEnergyAndCardPlays( args.Track );
		return TrackRevealed.InvokeAsync( args );
	}

	void CheckEnergyAndCardPlays( Track track ) {
		if(track.Energy.HasValue && EnergyPerTurn < track.Energy.Value)
			EnergyPerTurn = track.Energy.Value;
		if(track.CardPlay.HasValue && CardPlayCount < track.CardPlay.Value)
			CardPlayCount = track.CardPlay.Value;
	}

	#endregion

	#region Exposed Data

	public IEnumerable<Space> SacredSites() => SacredSiteStates.Downgrade();

	public virtual IEnumerable<SpaceState> SacredSiteStates => GameState.Current.AllActiveSpaces.Where( IsSacredSite );

	public int Total() => GameState.Current.AllSpaces.Sum( CountOn );

	/// <summary> All *Active* Spaces </summary>
	public IEnumerable<SpaceState> ActiveSpaceStates => GameState.Current.AllActiveSpaces.Where( IsOn );
//	IEnumerable<SpaceState> IKnowSpiritLocations.ActiveSpaceStates => ActiveSpaceStates;

	public IEnumerable<SpaceState> MovableSpaceStates => ActiveSpaceStates.Where( HasMovableTokens );


	#endregion Exposed Data

	public DualAsyncEvent<TrackRevealedArgs> TrackRevealed { get; } = new DualAsyncEvent<TrackRevealedArgs>();

	public SpiritPresenceToken Token { get; protected set; }

	#region Memento

	// Revealed Count + Placed.
	public virtual IMemento<SpiritPresence> SaveToMemento() => new Memento( this );
	public virtual void LoadFrom( IMemento<SpiritPresence> memento ) => ((Memento)memento).Restore( this );

	protected class Memento : IMemento<SpiritPresence> {
		public Memento( SpiritPresence src ) {
			energy = src.Energy.SaveToMemento();
			cardPlays = src.CardPlays.SaveToMemento();
			destroyed = src.Destroyed;
			energyPerTurn = src.EnergyPerTurn;
		}
		public void Restore( SpiritPresence src ) {
			src.Energy.LoadFrom( energy );
			src.CardPlays.LoadFrom( cardPlays );
			src.Destroyed = destroyed;
			src.EnergyPerTurn = energyPerTurn;
		}
		readonly IMemento<IPresenceTrack> energy;
		readonly IMemento<IPresenceTrack> cardPlays;
		readonly int destroyed;
		readonly int energyPerTurn;
	}

	#endregion
}
