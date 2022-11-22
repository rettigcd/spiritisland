namespace SpiritIsland;

public class SpiritPresence {

	#region constructors

	public SpiritPresence( IPresenceTrack energy, IPresenceTrack cardPlays ) {
		Energy = energy;
		CardPlays = cardPlays;

		Energy.TrackRevealed.Add( OnRevealed );
		CardPlays.TrackRevealed.Add( OnRevealed );

		foreach(var r in Energy.Revealed) CheckEnergyAndCardPlays( r );
		foreach(var r in CardPlays.Revealed) CheckEnergyAndCardPlays( r);
	}

	#endregion

	#region Tracks / Board

	public virtual IEnumerable<Track> RevealOptions(GameState _) 
		=> Energy.RevealOptions.Union( CardPlays.RevealOptions );

	public IEnumerable<Track> CoverOptions
		=> Energy.ReturnableOptions.Union( CardPlays.ReturnableOptions );

	public IReadOnlyCollection<Track> GetEnergyTrack() => Energy.Slots;

	public IReadOnlyCollection<Track> GetCardPlayTrack() => CardPlays.Slots;

	public int EnergyPerTurn { get; private set; } // !!! this needs saved with memento

	// ! These 2 tracks are only public so they can be accessed from Tests.
	// ! Not accessed from production code
	public IPresenceTrack Energy { get; }
	public IPresenceTrack CardPlays { get; }

	public int Destroyed { get; set; }

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

	#endregion

	#region Read-only properties

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
	public virtual bool CanBePlacedOn( TerrainMapper mapper, SpaceState ss ) => mapper.IsInPlay( ss.Space );
	public bool IsOn( SpaceState space ) => space[presenceToken] > 0;
	public virtual bool IsSacredSite( SpaceState space ) => 2 <= space[presenceToken];
	public int CountOn( SpaceState space ) => space[presenceToken];

	#endregion

	#region Game-Play things you can do with presence

	public virtual async Task Place( IOption from, Space to, GameState gs ) {
		await TakeFrom( from, gs );
		await PlaceOn( gs.Tokens[to] );
	}

	public async Task TakeFrom( IOption from, GameState gs ) {
		if(from is Track track) {
			await RevealTrack( track, gs );
		} else if(from is Space space) {
			if(Spaces(gs).Contains( space ))
				await RemoveFrom_NoCheck( space, gs );
			else
				throw new ArgumentException( "Can't pull from island space:" + from.ToString() );
		}
	}

	public async Task RemoveFrom( Space space, GameState gs ) {
		await RemoveFrom_NoCheck( space, gs );
		CheckIfSpiritIsDestroyed( gs );
	}

	public virtual Task ReturnDestroyedToTrack( Track dst, GameState gs ) {

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

	public void Move( Space from, Space to, GameState gs ) {
		RemoveFrom_NoCheck( from, gs );
		PlaceOn( gs.Tokens[to] );
	}

	public virtual async Task Destroy( Space space, GameState gs, DestoryPresenceCause actionType, AddReason blightAddedReason = AddReason.None ) {
		await DestroyBehavior.DestroyPresenceApi( this, space, gs, actionType, Guid.NewGuid() ); // !!! pass in the actionID!
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
		public virtual Task DestroyPresenceApi(SpiritPresence presence, Space space, GameState gs, DestoryPresenceCause actionType, Guid actionId ) {
			presence.RemoveFrom_NoCheck( space, gs );
			++presence.Destroyed;
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
	/// <param name="gs"></param>
	/// <returns></returns>
	public IReadOnlyCollection<SpaceState> Placed( GameState gs ) {

		// !!! this method is an abomination
		// Encapsulate this data.

		var placed = new List<SpaceState>();
		foreach(var space in gs.AllActiveSpaces.Where( IsOn ))
			for(int i = 0; i < CountOn( space ); ++i)
				placed.Add( space );
		return placed.AsReadOnly();
	}

	// one item for each space that has presence
	public IEnumerable<Space> Spaces( GameState gs ) {
		var ss = SpaceStates( gs ).ToArray();
		return ss.Select( x => x.Space );
	}
	public IEnumerable<SpaceState> SpaceStates( GameState gs ) => gs.AllActiveSpaces.Where( IsOn );

	#endregion Exposed Data

	#region Is this Setup or Game play?

	public virtual Task PlaceOn( SpaceState space ) {
		space.Adjust(presenceToken,1);
		return Task.CompletedTask;
	}
	public void Adjust( SpaceState space, int count ) {
		space.Adjust( presenceToken, count );
	}

	#endregion

	public DualAsyncEvent<TrackRevealedArgs> TrackRevealed { get; } = new DualAsyncEvent<TrackRevealedArgs>();

	/// <remarks>public so we can remove it for Replacing with Beast and advanced spirit strangness</remarks>
	// !!! This is called for 2 different reasons...
	// (1) To move presence to another location on the board - no End-of-Game check is necessary
	// (2) Presence is replaced with something else. End-of-Game check IS necessary.
	// Also - if we have presence in Stasis, then removing 2nd to last presence will INCORRECTLY trigger loss.
	protected virtual Task RemoveFrom_NoCheck( Space space, GameState gameState ) { 
		gameState.Tokens[space].Adjust(presenceToken,-1);
		return Task.CompletedTask;
	}

	UniqueToken presenceToken = new UniqueToken("Presence",TokenCategory.Presence);

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
		readonly Space[] placed;
		readonly IMemento<IPresenceTrack> energy;
		readonly IMemento<IPresenceTrack> cardPlays;
		readonly int destroyed;
		readonly int energyPerTurn;
	}

	#endregion
}

public enum DestoryPresenceCause {
	None,
	SpiritPower,    // One use of a Power;
	DahanDestroyed, // Thunderspeaker

	Blight,          // blight added to land
	BlightedIsland,  // Direct effect of the Blighted Island card


	// Not used
	Adversary,      //An Adversary's Escalation effect
	Event,          // Everything a Main/Token/Dahan Event does
	FearCard,       // Everything one Fear Card does
}
