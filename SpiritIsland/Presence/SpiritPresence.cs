namespace SpiritIsland;

public class SpiritPresence : IKnowSpiritLocations, ITokenClass {

	#region constructors

	public SpiritPresence( Spirit spirit,  IPresenceTrack energy, IPresenceTrack cardPlays )
		:this(spirit,energy,cardPlays,new SpiritPresenceToken(spirit),null){ }

	public SpiritPresence( Spirit spirit, IPresenceTrack energy, IPresenceTrack cardPlays, SpiritPresenceToken token )
		:this(spirit,energy,cardPlays,token,null){ }

	public SpiritPresence( Spirit spirit, IPresenceTrack energy, IPresenceTrack cardPlays, Incarna incarna )
		:this(spirit,energy,cardPlays,new SpiritPresenceToken(spirit),incarna) { }

	SpiritPresence( Spirit spirit, IPresenceTrack energy, IPresenceTrack cardPlays, SpiritPresenceToken token, Incarna incarna ) {
		Self = spirit;

		Energy = energy;
		CardPlays = cardPlays;

		Energy.TrackRevealed.Add( OnRevealed );
		CardPlays.TrackRevealed.Add( OnRevealed );

		InitEnergyAndCardPlays();

		Token = token;
		Incarna = incarna ?? new Incarna(spirit,"[-]",Img.None,Img.None); // a null Incarna
	}


	protected void InitEnergyAndCardPlays() {
		// !!! in unit tests, Revealed is sometimes empty
		if(Revealed.Any()) {
			EnergyPerTurn = Revealed.Max( x => x.Energy ?? 0 );    // Revealed includes card-track that has null energy (??0) which keeps us from going negative.
			CardPlayPerTurn = Revealed.Max( x => x.CardPlay ?? 0 );
		}
	}

	protected Spirit Self { get; }

	#endregion

	/// <summary> The normal spirit presence. </summary>
	public SpiritPresenceToken Token { get; }

	public Incarna Incarna { get; }

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
	public IEnumerable<IActOn<Spirit>> RevealedActions => Revealed
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

	virtual public IEnumerable<Track> RevealOptions() => Energy.RevealOptions.Union( CardPlays.RevealOptions );
	public IEnumerable<Track> CoverOptions => Energy.ReturnableOptions.Union( CardPlays.ReturnableOptions );

	IEnumerable<Track> Revealed => Energy.Revealed.Union(CardPlays.Revealed );
	public AsyncEvent<TrackRevealedArgs> TrackRevealed { get; } = new();

	public void AdjustEnergyTrack( int delta ) {
		// ctx.Self.EnergyCollected.Add( spirit => --spirit.Energy );
		if(delta == 0) return;
		foreach(Track t in Energy.Slots)
			if(t.Energy.HasValue)
				t.Energy += delta;

		InitEnergyAndCardPlays();
	}

	// ------------------------------
	// ----  Reveal / Return  -------
	// ------------------------------

	virtual protected async Task RevealTrack( Track track ) {
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
		EnergyPerTurn   = Math.Max( EnergyPerTurn, args.Track.Energy ?? 0 );
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

	#region nice Predicate methods 

	/// <summary>
	/// Specifies if the the given space is valid.
	/// </summary>
	virtual public bool CanBePlacedOn( SpaceState spaceState ) => ActionScope.Current.TerrainMapper.IsInPlay( spaceState.Space );

	virtual public bool IsSacredSite( SpaceState space ) => 2 <= CountOn(space);

	public bool IsOn( SpaceState spaceState ) => 0 < spaceState[Token] || HasIncarna(spaceState);

	// virtual public bool IsOn( Board board ) => Token.IsOn(board);
	public bool IsOn( Board board ) => Token.IsOn(board) 
		|| Incarna.IsPlaced && Incarna.Space.Space.Boards.Contains(board);

	public bool IsOnIsland => Token.IsOnIsland || Incarna.IsPlaced;

	public int CountOn( SpaceState spaceState ) => spaceState[Token] // For Mapper in a .Select(...)
		+ (HasIncarna(spaceState) ? 1 : 0);

	public IEnumerable<IToken> TokensDeployedOn( SpaceState space ) {
		// !!! TODO Replace calls to this with:     spaceState.OfTag(this);
		if(0 < space[Token]) yield return Token;
		if(0 < space[Incarna]) yield return Incarna;
	}


	bool HasIncarna(SpaceState ss) => ss == Incarna.Space;

	#endregion

	#region Moving Presenece between: Tracks <=> Boards <=> Destroyed

	public async Task DestroyPresenceOn( SpaceState spaceState ) {
		if(spaceState.Has( this ))
			await spaceState.Destroy( Token, 1 );
	}

	public int Destroyed {
		get => Token.Destroyed;
		set => Token.Destroyed = value;
	}

	/// <summary> Removes a Destroyed Presence from the game. </summary>
	public void RemoveDestroyed( int count ) {
		if(Destroyed < count) throw new ArgumentOutOfRangeException( nameof( count ) );
		Destroyed -= count;
	}


	/// <param name="from">Track, Space, or SpaceToken</param>
	public async Task PlaceAsync( IOption from, Space to ) {
		var token = await TakeFromAsync( from );
		await to.Tokens.AddAsync( token, 1 );
	}

	public async Task PlaceDestroyedAsync( int numToPlace, Space to ) {
		numToPlace = Math.Min( numToPlace, Destroyed );

		await to.Tokens.AddAsync( Token, numToPlace );
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
			await fromSpace.RemoveAsync( st.Token, 1, RemoveReason.Removed ); // This is not a .MovedFrom because that needs done from .Move
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
	public IEnumerable<SpaceState> SacredSites => Lands.Tokens().Where( IsSacredSite );
	public IEnumerable<SpaceState> SuperSacredSites => Lands.Tokens().Where( space => 3 <= CountOn( space ) );


	/// <summary> Existing Spaces </summary>
	/// <remarks> Determining presence locations does NOT require Tokens so default type is Space. </remarks>
	public IEnumerable<Space> Lands => Incarna.IsPlaced
		? Token.Spaces_Existing.Include( Incarna.Space.Space )
		: Token.Spaces_Existing;


	public IEnumerable<SpaceToken> Deployed => Incarna.IsPlaced 
		? Token.Deployed.Include( Incarna.AsSpaceToken() )
		: Token.Deployed;

	public IEnumerable<SpaceToken> Movable => CanMove ? Deployed : Enumerable.Empty<SpaceToken>();

	/// <summary> Unfiltered </summary>
	public int TotalOnIsland() => GameState.Current.Spaces_Unfiltered.Sum( CountOn );

	#endregion Exposed Data

	#region ITokenClass Imp
	string ITokenClass.Label => "Presence";
	bool ITokenClass.HasTag( ITag tag ) => tag == this // Spirit.Presence acts like the class for the Spirit
		|| tag == TokenCategory.Presence; // !! Should be on Incarna Also
	#endregion ITokenClass Imp

	#region Memento

	// Revealed Count + Placed.
	public virtual IMemento<SpiritPresence> SaveToMemento() => new Memento( this );
	public virtual void LoadFrom( IMemento<SpiritPresence> memento ) => ((Memento)memento).Restore( this );


	protected class Memento : IMemento<SpiritPresence> {
		public Memento( SpiritPresence src ) {
			_energy = src.Energy.SaveToMemento();
			_cardPlays = src.CardPlays.SaveToMemento();
			_destroyed = src.Destroyed;
			_lowestTrackEnergy = FirstEnergyTrackValue( src );
			_incarnaEmpowered = src.Incarna.Empowered;
			// don't need to save Space because that gets set via Tokens and ITrackMySpaces
		}

		virtual public void Restore( SpiritPresence src ) {
			src.Energy.LoadFrom( _energy );
			src.CardPlays.LoadFrom( _cardPlays );
			src.Destroyed = _destroyed;
			src.Incarna.Empowered = _incarnaEmpowered;
			// don't need to restore Space because that gets set via Tokens and ITrackMySpaces
			src.AdjustEnergyTrack( _lowestTrackEnergy /* what it should be */ - FirstEnergyTrackValue(src) /* what it is */ );
		}

		static int FirstEnergyTrackValue( SpiritPresence src ) => src.Energy.Revealed.First().Energy.Value; // The first one should always have an energy value.

		readonly IMemento<IPresenceTrack> _energy;
		readonly IMemento<IPresenceTrack> _cardPlays;
		readonly int _destroyed;
		readonly int _lowestTrackEnergy;
		readonly bool _incarnaEmpowered;
	}

	#endregion
}
