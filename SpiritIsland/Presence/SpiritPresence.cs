namespace SpiritIsland;

public class SpiritPresence : IKnowSpiritLocations {

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

	public virtual IEnumerable<Track> RevealOptions 
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

	public virtual IEnumerable<Space> SacredSites( TerrainMapper _ ) => Placed
		.GroupBy( x => x )
		.Where( grp => grp.Count() > 1 )
		.Select( grp => grp.Key );

	#endregion

	#region Game-Play things you can do with presence

	public virtual async Task Place( IOption from, Space to, GameState gs ) {
		await TakeFrom( from, gs );
		await PlaceOn( to, gs );
	}

	public async Task TakeFrom( IOption from, GameState gs ) {
		if(from is Track track) {
			await RevealTrack( track, gs );
		} else if(from is Space space) {
			if(Spaces.Contains( space ))
				await RemoveFrom_NoCheck( space, gs );
			else
				throw new ArgumentException( "Can't pull from island space:" + from.ToString() );
		}
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

	public virtual Task ReturnDestroyedToTrack( Track dst, GameState gs ) {

		// src / from
		if(Destroyed <=0 ) throw new InvalidOperationException("There is no Destroyed presence to return.");
		--Destroyed;

		// !!! This should reduce card plays or energy but I can't find that anywhere.

		// To
		return Return(dst);
	}

	public Task Return( Track dst ) {
		return (Energy.Return( dst ) || CardPlays.Return( dst ))
			? Task.CompletedTask
			: throw new ArgumentException( "Unable to find location to restore presence" );
	}

	public void Move( Space from, Space to, GameState gs ) {
		RemoveFrom_NoCheck( from, gs );
		PlaceOn( to, gs );
	}

	public virtual async Task Destroy( Space space, GameState gs, DestoryPresenceCause actionType, AddReason blightAddedReason = AddReason.None ) {
		await DestroyBehavior.DestroyPresenceApi(this,space,gs, actionType, Guid.NewGuid() ); // !!! pass in the actionID!
		CheckIfSpiritIsDestroyed();
	}

	void CheckIfSpiritIsDestroyed() {
		if(Placed.Count == 0 && stasis.Count == 0 )
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

	#region Setup / Adjust

	public async Task RemoveFrom( Space space, GameState gs ) {
		await RemoveFrom_NoCheck( space, gs );
		CheckIfSpiritIsDestroyed();
	}

	public void PutInStasis( Space space, GameState gs ) {
		while( IsOn(space)) {
			RemoveFrom_NoCheck( space, gs );
			stasis.Add(space);
		}
	}

	public void ReleaseFromStasis( Space space, GameState gs ) {
		while( stasis.Contains(space)) {
			stasis.Remove(space);
			PlaceOn(space, gs);
		}
	}

	#endregion

	#region Per Turn stuff

	public int CardPlayCount { get; private set; }

	public ElementCounts AddElements( ElementCounts elements=null ) {
		if(elements==null) elements = new ElementCounts();
		Energy.AddElements( elements );
		CardPlays.AddElements( elements);
		return elements;
	}

	#endregion

	/// <summary>
	/// Specifies if the the given space is valid.
	/// </summary>
	public Func<Space,bool> IsValid = DefaultIsValid; // override for Lure, Ocean, Volcano

	static bool DefaultIsValid(Space space) => space.IsInPlay;

	public DualAsyncEvent<TrackRevealedArgs> TrackRevealed { get; } = new DualAsyncEvent<TrackRevealedArgs>();

	public IReadOnlyCollection<Space> Placed => placed.AsReadOnly();

	public IEnumerable<Space> Spaces => placed.Distinct();

	public int CountOn( Space space ) => placed.Count( p => p == space );

	public bool IsOn( Space space ) => placed.Contains( space );

	public virtual Task PlaceOn(Space space, GameState gameState) { 
		placed.Add(space);
		return Task.CompletedTask;
		// return gameState.Tokens[space].Add(Token,1, AddReason.Added); // !!! this add reason might not be correct
	}

	/// <remarks>public so we can remove it for Replacing with Beast and advanced spirit strangness</remarks>
	// !!! This is called for 2 different reasons...
	// (1) To move presence to another location on the board - no End-of-Game check is necessary
	// (2) Presence is replaced with something else. End-of-Game check IS necessary.
	// Also - if we have presence in Stasis, then removing 2nd to last presence will INCORRECTLY trigger loss.
	protected virtual Task RemoveFrom_NoCheck( Space space, GameState gameState ) { 
		placed.Remove( space );
		return Task.CompletedTask;
//			return gameState.Tokens[space].Remove(Token,1); // !!! Do we event want to generate an event?
	}

	public void Adjust( Space space, int count ) {
		while( 0 < count) {
			placed.Add( space );
			--count;
		}
		while( count < 0 ) {
			placed.Remove( space );
			++count;
		}
	}

	readonly List<Space> placed = new List<Space>();
	readonly List<Space> stasis = new List<Space>();


	#region Memento

	// Revealed Count + Placed.
	public virtual IMemento<SpiritPresence> SaveToMemento() => new Memento(this);
	public virtual void LoadFrom( IMemento<SpiritPresence> memento ) => ((Memento)memento).Restore(this);

	protected class Memento : IMemento<SpiritPresence> {
		public Memento(SpiritPresence src) {
			placed = src.placed.ToArray();
			energy = src.Energy.SaveToMemento();
			cardPlays = src.CardPlays.SaveToMemento();
			destroyed = src.Destroyed;
			energyPerTurn = src.EnergyPerTurn;
		}
		public void Restore(SpiritPresence src ) {
			src.placed.Clear(); src.placed.AddRange( placed );
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
