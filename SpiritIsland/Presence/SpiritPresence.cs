using SpiritIsland.Select;

namespace SpiritIsland;

public class SpiritPresence : IKnowSpiritLocations {

	#region constructors

	public SpiritPresence( IPresenceTrack energy, IPresenceTrack cardPlays ) {
		Energy = energy;
		CardPlays = cardPlays;

		Energy.TrackRevealed.Add( OnRevealed );
		CardPlays.TrackRevealed.Add( OnRevealed );

		InitEnergyAndCardPlays();

	}

	protected void InitEnergyAndCardPlays() {
		// !!! in unit tests, Revealed is sometimes empty
		if(Revealed.Any()) {
			EnergyPerTurn = Revealed.Max( x => x.Energy ?? 0 );
			CardPlayPerTurn = Revealed.Max( x => x.CardPlay ?? 0 );
		}
	}

	public virtual void SetSpirit( Spirit spirit ) {
		if(Self != null) throw new InvalidOperationException();
		Self = spirit;
		Token = new SpiritPresenceToken(spirit);
	}

	protected Spirit Self { get; set; }

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
	public IEnumerable<IActionFactory> RevealedActions => Revealed
		.Select( x => x.Action )
		.Where( x => x != null );

	public ElementCounts TrackElements {
		get {
			var elements = new ElementCounts();
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

	public async Task DestroyPresenceOn( SpaceState spaceState ) {
		if(spaceState.Has( Token ))
			await spaceState.Destroy( Token, 1 );
	}

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

	#region Destroyed

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

	#region nice Predicate methods 

	/// <summary>
	/// Specifies if the the given space is valid.
	/// </summary>
	public virtual bool CanBePlacedOn( SpaceState spaceState ) => ActionScope.Current.TerrainMapper.IsInPlay( spaceState.Space );

	public virtual bool IsSacredSite( SpaceState space ) => 2 <= space[Token];

	public bool IsOn( SpaceState spaceState ) => 0 < spaceState[Token]; // For Predicate in a .Where(...)
	public bool IsOn( Board board ) => GameState.Current.Tokens.IsOn(Token,board);
	public bool IsOnIsland => GameState.Current.Tokens.IsOnAnyBoard( Token );

	public int CountOn( SpaceState spaceState ) => spaceState[Token]; // For Mapper in a .Select(...)

	public IEnumerable<IToken> TokensDeployedOn( SpaceState space ) { if(IsOn( space )) yield return Token; }

	#endregion

	#region Game-Play things you can do with presence

	/// <param name="from">Track, Space, or SpaceToken</param>
	public async Task Place( IOption from, Space to ) {
		var token = await TakeFrom( from );
		await to.Tokens.Add( token, 1 );
	}

	/// <param name="from">Track, Space, or SpaceState</param>
	/// <returns>Token that is being 'taken'.</returns>
	public async Task<IToken> TakeFrom( IOption from ) {
		if(from is Track track)
			await RevealTrack( track );
		else if(from is Space space)
			await TakeFromSpace( space );
		else if(from is SpaceToken spaceToken) {
			await TakeFromSpace( spaceToken.Space );
			return spaceToken.Token;
		}
		return Token;
	}

	async Task TakeFromSpace( Space space ) {
		SpaceState fromSpace = space.Tokens;
		if(fromSpace.Has(Token))
			await fromSpace.Remove( Token, 1, RemoveReason.Removed ); // This is not a .MovedFrom because that needs done from .Move
		else
			throw new ArgumentException( "Can't pull from island space:" + space.ToString() );
	}

	public Task ReturnDestroyedToTrack( Track dst ) {

		// src / from
		if(Destroyed <= 0) throw new InvalidOperationException( "There is no Destroyed presence to return." );
		--Destroyed;

		// To
		return Return( dst );
	}

	/// <remarks>Convenience - checks CanMove and token on space</remarks>
	public bool HasMovableTokens( SpaceState spaceState ) => CanMove && spaceState.Has(Token);

	public bool CanMove { get; set; } = true; // Spirit effect - Settle Into Hunting Grounds

	#endregion

	#region Exposed Data

	/// <summary> Existing </summary>
	/// <remarks> Determining SS requires Tokens so default type for SS is SpaceState. </remarks>
	public IEnumerable<SpaceState> SacredSites => Spaces.Tokens().Where( IsSacredSite );

	/// <summary> Existing Spaces </summary>
	/// <remarks> Determining presence locations does NOT require Tokens so default type is Space. </remarks>
	public IEnumerable<Space> Spaces => GameState.Current.Tokens.Spaces_Existing( Token );

	public IEnumerable<SpaceToken> Deployed => Spaces.Select( (Func<Space, SpaceToken>)(s=>new SpaceToken( s, (IToken)this.Token)));
	public IEnumerable<SpaceToken> Movable => CanMove ? Deployed : Enumerable.Empty<SpaceToken>();

	/// <summary> Unfiltered </summary>
	public int TotalOnIsland() => GameState.Current.Spaces_Unfiltered.Sum( CountOn );

	#endregion Exposed Data

	public AsyncEvent<TrackRevealedArgs> TrackRevealed { get; } = new();

	/// <summary> The normal spirit presence. </summary>
	public SpiritPresenceToken Token { 	get; set; }

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
