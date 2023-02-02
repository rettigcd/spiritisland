namespace SpiritIsland;

// == Presence/GameState Strategy ==
// * If GameState is only used for getting SpaceState, pass in SpaceState instead. (1 parameter is better than 2)
// Methods on SpiritPresence should only be called from BoundedPresence aka PresenceState

public class SpiritPresence {

	#region constructors

	public SpiritPresence( IPresenceTrack energy, IPresenceTrack cardPlays ) {
		Energy = energy;
		CardPlays = cardPlays;

		Energy.TrackRevealed.Add( OnRevealed );
		CardPlays.TrackRevealed.Add( OnRevealed );

		InitEnergyAndCardPlays();

		this.Token = new SpiritPresenceToken();
	}

	protected void InitEnergyAndCardPlays() {
		foreach(var r in Energy.Revealed) CheckEnergyAndCardPlays( r );
		foreach(var r in CardPlays.Revealed) CheckEnergyAndCardPlays( r );
	}

	public virtual void SetSpirit( Spirit spirit ) {
		if(Self!=null) throw new InvalidOperationException();
		Self = spirit;
	}

	protected Spirit Self { get; set; }

	#endregion

	#region Tracks / Board

	public virtual IEnumerable<Track> RevealOptions(GameState _) 
		=> Energy.RevealOptions.Union( CardPlays.RevealOptions );

	public IEnumerable<Track> CoverOptions
		=> Energy.ReturnableOptions.Union( CardPlays.ReturnableOptions );

	public int EnergyPerTurn { get; private set; }

	// ! These 2 tracks are only public so they can be accessed from Tests.
	// ! Not accessed from production code
	public IPresenceTrack Energy { get; }
	public IPresenceTrack CardPlays { get; }

	public int Destroyed { get; set; }

	/// <summary> Removes a Destroyed Presence from the game. </summary>
	public void RemoveDestroyed( int count ) {
		if(count>Destroyed) throw new ArgumentOutOfRangeException(nameof(count));
		Destroyed -= count;
	}

	#endregion

	#region Readonly 

	public IEnumerable<IActionFactory> RevealedActions 
		=> CardPlays.Revealed
			.Union(Energy.Revealed)
			.Select(x => x.Action)
			.Where(x => x != null);

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
	public virtual bool CanBePlacedOn( SpaceState spaceState, TerrainMapper mapper ) => mapper.IsInPlay( spaceState );
	public bool IsOn( SpaceState spaceState ) => 0 < spaceState[Token];
	public virtual bool IsSacredSite( SpaceState space ) => 2 <= space[Token];
	public int CountOn( SpaceState spaceState ) => spaceState[Token];

	#endregion

	#region Game-Play things you can do with presence

	public virtual async Task Place( IOption from, Space to, GameState gs, UnitOfWork actionScope ) {
		await TakeFrom( from, gs );
		await PlaceOn( gs.Tokens[to], actionScope ); 
	}

	public async Task TakeFrom( IOption from, GameState gs ) {
		if(from is Track track) {
			await RevealTrack( track, gs );
		} else if(from is Space space) {
			var fromSpace = gs.Tokens[space];
			if(IsOn(fromSpace))
				await RemoveFrom_NoCheck( fromSpace );
			else
				throw new ArgumentException( "Can't pull from island space:" + from.ToString() );
		}
	}

	public async Task RemoveFrom( Space space, GameState gs ) {
		await RemoveFrom_NoCheck( gs.Tokens[space] );
		CheckIfSpiritIsDestroyed( gs );
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

	public bool CanMove { get; set; } = true;

	public async Task Move( Space from, Space to, GameState gs, UnitOfWork actionScope ) {
		await RemoveFrom_NoCheck( gs.Tokens[from] );
		await PlaceOn( gs.Tokens[to], actionScope );
	}

	public virtual async Task Destroy( Space space, GameState gs, int count, DestoryPresenceCause actionType, UnitOfWork actionScope, AddReason blightAddedReason = AddReason.None ) {
		await DestroyBehavior.DestroyPresenceApi( this, space, gs, count, actionType, actionScope );
		CheckIfSpiritIsDestroyed( gs );
	}

	protected virtual async Task RevealTrack( Track track, GameState gs ) {
		if(track == Track.Destroyed && Destroyed > 0)
			--Destroyed;
		else{
			bool energyRevealed = await Energy.Reveal( track, gs );
			if(!energyRevealed) {
				bool cardRevealed = await CardPlays.Reveal( track, gs );
				if( !cardRevealed )
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

	void CheckIfSpiritIsDestroyed(GameState gs) {
		if(!gs.AllSpaces.Any(IsOn))
			GameOverException.Lost( "Spirit is Destroyed" ); // !! if we had access to the Spirit here, we could say who it was.
	}

	public IDestroyPresenceBehavour DestroyBehavior = new DefaultDestroyBehavior(); // replaceable / plugable

	public class DefaultDestroyBehavior : IDestroyPresenceBehavour {
		public virtual Task DestroyPresenceApi(SpiritPresence presence, Space space, GameState gs, int count, DestoryPresenceCause actionType, UnitOfWork actionScope ) {
			presence.RemoveFrom_NoCheck( gs.Tokens[space], count );
			presence.Destroyed += count;
			return Task.CompletedTask;
		}
	}

	#endregion

	#region Exposed Data

	public IEnumerable<Space> SacredSites( GameState gs, TerrainMapper tm ) 
		=> SacredSiteStates(gs,tm).Select( s => s.Space );
	public virtual IEnumerable<SpaceState> SacredSiteStates( GameState gs, TerrainMapper _ ) => gs.AllActiveSpaces
		.Where( IsSacredSite );

	/// <summary>
	/// One item for each presence
	/// </summary>
	/// <returns>1 item for each token on each space</returns>
	public IReadOnlyCollection<SpaceState> Placed( GameState gs ) {

		// !!! this method is an abomination
		// Deprecate!

		var placed = new List<SpaceState>();
		foreach(var space in gs.AllActiveSpaces.Where( IsOn ))
			for(int i = 0; i < CountOn( space ); ++i)
				placed.Add( space );
		return placed.AsReadOnly();
	}

	// !!! deprecate one item for each space that has presence
	public IEnumerable<Space> Spaces( GameState gs ) => SpaceStates( gs ).Select( x => x.Space );

	public IEnumerable<SpaceState> SpaceStates( GameState gs ) => gs.AllActiveSpaces.Where( IsOn );

	#endregion Exposed Data

	#region Is this Setup or Game play?

	public async virtual Task PlaceOn( SpaceState space, UnitOfWork actionScope ) {
		await space.Bind( actionScope ).Add(Token,1);
	}
	public virtual void Adjust( SpaceState space, int count ) {
		space.Adjust( Token, count );
	}

	#endregion

	public DualAsyncEvent<TrackRevealedArgs> TrackRevealed { get; } = new DualAsyncEvent<TrackRevealedArgs>();

	/// <remarks>public so we can remove it for Replacing with Beast and advanced spirit strangness</remarks>
	// !!! This is called for 2 different reasons... repace with Tokens Add/Remove/Adjust
	// (1) To move presence to another location on the board - no End-of-Game check is necessary
	// (2) Presence is replaced with something else. End-of-Game check IS necessary.
	protected virtual Task RemoveFrom_NoCheck( SpaceState space, int count=1 ) { 
		space.Adjust(Token,-count);
		return Task.CompletedTask;
	}

	public SpiritPresenceToken Token { get; protected set; }

	#region Memento

	// Revealed Count + Placed.
	public virtual IMemento<SpiritPresence> SaveToMemento() => new Memento(this);
	public virtual void LoadFrom( IMemento<SpiritPresence> memento ) => ((Memento)memento).Restore(this);

	protected class Memento : IMemento<SpiritPresence> {
		public Memento(SpiritPresence src) {
			energy = src.Energy.SaveToMemento();
			cardPlays = src.CardPlays.SaveToMemento();
			destroyed = src.Destroyed;
			energyPerTurn = src.EnergyPerTurn;
		}
		public void Restore(SpiritPresence src ) {
			src.Energy.LoadFrom(energy);
			src.CardPlays.LoadFrom(cardPlays);
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
