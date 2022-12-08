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

	public IReadOnlyCollection<Track> GetEnergyTrack() => Energy.Slots;

	public IReadOnlyCollection<Track> GetCardPlayTrack() => CardPlays.Slots;

	public int EnergyPerTurn { get; private set; } // !!! this needs saved with memento

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
	public virtual bool CanBePlacedOn( SpaceState spaceState, TerrainMapper mapper ) => mapper.IsInPlay( spaceState.Space );
	public bool IsOn( SpaceState spaceState ) => spaceState[Token] > 0;
	public virtual bool IsSacredSite( SpaceState space ) => 2 <= space[Token];
	public int CountOn( SpaceState spaceState ) => spaceState[Token];

	#endregion

	#region Game-Play things you can do with presence

	public virtual async Task Place( IOption from, Space to, GameState gs, UnitOfWork actionId ) {
		await TakeFrom( from, gs );
		await PlaceOn( gs.Tokens[to], actionId ); 
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

	public async Task Move( Space from, Space to, GameState gs, UnitOfWork actionId ) {
		await RemoveFrom_NoCheck( gs.Tokens[from] );
		await PlaceOn( gs.Tokens[to], actionId );
	}

	public virtual async Task Destroy( Space space, GameState gs, DestoryPresenceCause actionType, UnitOfWork actionId, AddReason blightAddedReason = AddReason.None ) {
		await DestroyBehavior.DestroyPresenceApi( this, space, gs, actionType, actionId );
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
		public virtual Task DestroyPresenceApi(SpiritPresence presence, Space space, GameState gs, DestoryPresenceCause actionType, UnitOfWork actionId ) {
			presence.RemoveFrom_NoCheck( gs.Tokens[space] );
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

	// !!! one item for each space that has presence
	public IEnumerable<Space> Spaces( GameState gs ) {
		var ss = SpaceStates( gs ).ToArray();
		return ss.Select( x => x.Space );
	}

	public IEnumerable<SpaceState> SpaceStates( GameState gs ) => gs.AllActiveSpaces.Where( IsOn );

	#endregion Exposed Data

	#region Is this Setup or Game play?

	public async virtual Task PlaceOn( SpaceState space, UnitOfWork actionId ) {
		await space.Add(Token,1,actionId);
	}
	public virtual void Adjust( SpaceState space, int count ) {
		space.Adjust( Token, count );
	}

	#endregion

	public DualAsyncEvent<TrackRevealedArgs> TrackRevealed { get; } = new DualAsyncEvent<TrackRevealedArgs>();

	/// <remarks>public so we can remove it for Replacing with Beast and advanced spirit strangness</remarks>
	// !!! This is called for 2 different reasons...
	// (1) To move presence to another location on the board - no End-of-Game check is necessary
	// (2) Presence is replaced with something else. End-of-Game check IS necessary.
	// Also - if we have presence in Stasis, then removing 2nd to last presence will INCORRECTLY trigger loss.
	protected virtual Task RemoveFrom_NoCheck( SpaceState space ) { 
		space.Adjust(Token,-1);
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

public class SpiritPresenceToken : Token, TokenClass {

	#region private
//	static int tokenTypeCount; // so each spirit presence gets a different number
	#endregion

	public SpiritPresenceToken() {
		// Text = "P" + (tokenTypeCount++); // !! This kind of sucks. Could be based on Spirit or starting Board-name
		// !!! DeployedPresenceDecisoin: IslandControl needs access to the PresenceToken so it can record the Location for creating hotspots during.
		Text = "Presence";      // !!! this only works in SOLO.
	}

	#region Token parts

	public TokenClass Class => this;

	public string Text { get; }

	public string SpaceAbreviation => null; // hide it

	#endregion

	#region TokenClass parts
	string TokenClass.Label => "Presence";
	TokenCategory TokenClass.Category => TokenCategory.Presence;
	#endregion
}

public enum DestoryPresenceCause {
	None,
	SpiritPower,    // One use of a Power;
	DahanDestroyed, // Thunderspeaker
	SkipInvaderAction, // A Spread of Rampant Green

	Blight,          // blight added to land
	BlightedIsland,  // Direct effect of the Blighted Island card


	// Not used
	Adversary,      //An Adversary's Escalation effect
	Event,          // Everything a Main/Token/Dahan Event does
	FearCard,       // Everything one Fear Card does
}
