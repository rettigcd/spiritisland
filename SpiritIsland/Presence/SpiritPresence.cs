namespace SpiritIsland;

public class SpiritPresence : IKnowSpiritLocations, ITokenClass {

	#region constructors

	public SpiritPresence( Spirit spirit, IPresenceTrack energy, IPresenceTrack cardPlays, SpiritPresenceToken token = null )	{
		Self = spirit;

		Energy = energy;
		CardPlays = cardPlays;

		Energy.TrackRevealed.Add( OnRevealed );
		CardPlays.TrackRevealed.Add( OnRevealed );

		InitEnergyAndCardPlays();

		Token = token ?? new SpiritPresenceToken(spirit);
	}


	protected void InitEnergyAndCardPlays() {
		// !!! in unit tests, Revealed is sometimes empty
		if(Revealed.Any()) {
			EnergyPerTurn = Revealed.Max( x => x.Energy ?? 0 );
			CardPlayPerTurn = Revealed.Max( x => x.CardPlay ?? 0 );
		}
	}

	protected Spirit Self { get; }

	#endregion

	#region Tracks / Board

	// # of Energy & CardPlays  / Tern
	public int EnergyPerTurn { get; private set; }
	public int CardPlayPerTurn { get; private set; }

	// ------------------------------
	// ----  Presence Tracks  -------
	// ------------------------------

	// (These 2 tracks are only public so they can be accessed from Tests. - Not accessed from production code.)
	public IPresenceTrack Energy { get; }
	public IPresenceTrack CardPlays { get; }

	// ------------------------------
	// ----  Aggregate Track  -------
	// ------------------------------
	public IEnumerable<IActOn<SelfCtx>> RevealedActions => Revealed
		.Select( x => x.Action )
		.Where( x => x != null );

	public CountDictionary<Element> TrackElements {
		get {
			var elements = new CountDictionary<Element>();
			Energy.AddElementsTo( elements );
			CardPlays.AddElementsTo( elements );
			return elements;
		}
	}

	public virtual IEnumerable<Track> RevealOptions() => Energy.RevealOptions.Union( CardPlays.RevealOptions );
	public IEnumerable<Track> CoverOptions => Energy.ReturnableOptions.Union( CardPlays.ReturnableOptions );

	IEnumerable<Track> Revealed => Energy.Revealed.Union(CardPlays.Revealed );

	// ------------------------------
	// ----  Reveal / Return  -------
	// ------------------------------

	protected virtual async Task RevealTrack( Track track ) {
		if(track == Track.Destroyed && 0 < Destroyed)
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
		EnergyPerTurn = Math.Max( EnergyPerTurn, args.Track.Energy ?? 0 );
		CardPlayPerTurn = Math.Max( CardPlayPerTurn, args.Track.CardPlay ?? 0 );
		return TrackRevealed.InvokeAsync( args );
	}

	public Task Return( Track dst ) {

		// Rescan Energy & CardPlays
		InitEnergyAndCardPlays();

		return (Energy.Return( dst ) || CardPlays.Return( dst ))
			? Task.CompletedTask
			: throw new ArgumentException( "Unable to find location to restore presence" );
	}

	#endregion

	public async Task DestroyPresenceOn( SpaceState spaceState ) {
		if(spaceState.Has( this ))
			await spaceState.Destroy( Token, 1 );
	}

	#region Destroyed

	public int Destroyed {
		get => Token.Destroyed;
		set => Token.Destroyed = value;
	}

	/// <summary> Removes a Destroyed Presence from the game. </summary>
	public void RemoveDestroyed( int count ) {
		if(Destroyed < count) throw new ArgumentOutOfRangeException( nameof( count ) );
		Destroyed -= count;
	}

	#endregion

	#region nice Predicate methods 

	/// <summary>
	/// Specifies if the the given space is valid.
	/// </summary>
	public virtual bool CanBePlacedOn( SpaceState spaceState ) => ActionScope.Current.TerrainMapper.IsInPlay( spaceState.Space );

	public virtual bool IsSacredSite( SpaceState space ) => 2 <= CountOn(space);

	virtual public bool IsOn( SpaceState spaceState ) => 0 < spaceState[Token]; // For Predicate in a .Where(...)
	virtual public bool IsOn( Board board ) => Token.IsOn(board);
	virtual public bool IsOnIsland => Token.IsOnIsland; // !!?? are we overriding to include Incarna?

	virtual public int CountOn( SpaceState spaceState ) => spaceState[Token]; // For Mapper in a .Select(...)

	virtual public IEnumerable<IToken> TokensDeployedOn( SpaceState space ) { if(IsOn( space )) yield return Token; }

	#endregion

	#region Game-Play things you can do with presence

	/// <param name="from">Track, Space, or SpaceToken</param>
	public async Task PlaceAsync( IOption from, Space to ) {
		var token = await TakeFromAsync( from );
		await to.Tokens.Add( token, 1 );
	}

	public async Task PlaceDestroyedAsync( int numToPlace, Space to ) {
		numToPlace = Math.Min( numToPlace, Destroyed );

		await to.Tokens.Add( Token, numToPlace );
		Destroyed -= numToPlace;
	}

	/// <param name="from">Track, Space, or SpaceState</param>
	/// <returns>Token that is being 'taken'.</returns>
	public async Task<IToken> TakeFromAsync( IOption from ) {
		if(from is Track track)
			await RevealTrack( track );
		else if(from is Space space)
			await TakeFromSpaceAsync( Token.On(space) );
		else if(from is SpaceToken spaceToken) {
			await TakeFromSpaceAsync( spaceToken );
			return spaceToken.Token;
		}
		return Token;
	}

	static async Task TakeFromSpaceAsync( SpaceToken st ) {
		SpaceState fromSpace = st.Space.Tokens;
		if(0<fromSpace[st.Token])
			await fromSpace.Remove( st.Token, 1, RemoveReason.Removed ); // This is not a .MovedFrom because that needs done from .Move
		else
			throw new ArgumentException( "Can't pull from island space:" + st.ToString() );
	}

	public Task ReturnDestroyedToTrackAsync( Track dst ) {

		// src / from
		if(Destroyed <= 0) throw new InvalidOperationException( "There is no Destroyed presence to return." );
		--Destroyed;

		// To
		return Return( dst );
	}

	/// <remarks>Convenience - checks CanMove and token on space</remarks>
	public bool HasMovableTokens( SpaceState spaceState ) => CanMove && spaceState.Has(this);

	public bool CanMove { get; set; } = true; // Spirit effect - Settle Into Hunting Grounds

	#endregion

	#region Exposed Data

	/// <summary> Existing </summary>
	/// <remarks> Determining SS requires Tokens so default type for SS is SpaceState. </remarks>
	public IEnumerable<SpaceState> SacredSites => Spaces.Tokens().Where( IsSacredSite );
	public IEnumerable<SpaceState> SuperSacredSites => Spaces.Tokens().Where( space => 3 <= CountOn( space ) );


	/// <summary> Existing Spaces </summary>
	/// <remarks> Determining presence locations does NOT require Tokens so default type is Space. </remarks>
	virtual public IEnumerable<Space> Spaces => Token.Spaces_Existing;

	virtual public IEnumerable<SpaceToken> Deployed => Token.Deployed; // don't use .Spaces because it gets overriden to include non-token spaces

	public IEnumerable<SpaceToken> Movable => CanMove ? Deployed : Enumerable.Empty<SpaceToken>();

	/// <summary> Unfiltered </summary>
	public int TotalOnIsland() => GameState.Current.Spaces_Unfiltered.Sum( CountOn );

	#endregion Exposed Data

	public AsyncEvent<TrackRevealedArgs> TrackRevealed { get; } = new();

	/// <summary> The normal spirit presence. </summary>
	public SpiritPresenceToken Token { get; }


	#region ITokenClass Imp
	string ITokenClass.Label => "Presence";
	bool ITokenClass.HasTag( ITag tag ) => tag == this || tag == TokenCategory.Presence; // for both class and for token.
	#endregion ITokenClass Imp

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
		virtual public void Restore( SpiritPresence src ) {
			src.Energy.LoadFrom( energy );
			src.CardPlays.LoadFrom( cardPlays );
			src.Destroyed = destroyed;
			src.EnergyPerTurn = energyPerTurn;
		}
		readonly IMemento<IPresenceTrack> energy;
		readonly IMemento<IPresenceTrack> cardPlays;
		readonly int destroyed;
		readonly int energyPerTurn;
		public bool EmpoweredIncarna;
	}

	#endregion
}
