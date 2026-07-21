namespace SpiritIsland;

public class SpiritPresence : IKnowSpiritLocations, ITokenClass, IHaveMemento {

	#region constructors

	/// <summary> Constructs a spirit with: normal Presence Token and no Incarna</summary>
	public SpiritPresence( Spirit spirit,  IPresenceTrack energy, IPresenceTrack cardPlays )
		:this(spirit,energy,cardPlays,new SpiritPresenceToken(spirit),null){ }

	/// <summary> Constructs a spirit with: Special Presence Token </summary>
	public SpiritPresence( Spirit spirit, IPresenceTrack energy, IPresenceTrack cardPlays, SpiritPresenceToken token )
		:this(spirit,energy,cardPlays,token, incarna:null) { }

	/// <summary> Constructs a spirit with: Incarna </summary>
	public SpiritPresence( Spirit spirit, IPresenceTrack energy, IPresenceTrack cardPlays, Incarna incarna )
		:this(spirit,energy,cardPlays,new SpiritPresenceToken(spirit),incarna) { }

	public SpiritPresence( Spirit spirit, IPresenceTrack energy, IPresenceTrack cardPlays, SpiritPresenceToken token, Incarna? incarna ) {
		Self = spirit;

		Energy = energy;
		CardPlays = cardPlays;

		Energy.TrackRevealedAsync += OnTrackRevealedAsync;
		CardPlays.TrackRevealedAsync += OnTrackRevealedAsync;

		InitEnergyAndCardPlays();

		Token = token;
		Incarna = incarna ?? new Incarna(spirit,"[-]",Img.None,Img.None); // a null Incarna
		Destroyed = new DestroyedPresence(Token);
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

	public DestroyedPresence Destroyed { get; }

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
	
	
	/// <summary>
	/// Presence track Actions associated with revealed slots.  
	/// Executed each Spirit Phase immediately after energy is gained.
	/// </summary>
	public IEnumerable<IActOn<Spirit>> RevealedActions => Revealed
		.Select( x => x.Action )
		.OfType<IActOn<Spirit>>();

	public CountDictionary<Element> TrackElements {
		get {
			var elements = new CountDictionary<Element>();
			Energy.AddElementsTo( elements );
			CardPlays.AddElementsTo( elements );
			return elements;
		}
	}

	virtual public IEnumerable<ITokenLocation> RevealOptions() 
		=> Energy.RevealOptions
			.Union( CardPlays.RevealOptions )
			.Select(t=>new TrackPresence(t,Token));

	public IEnumerable<Track> CoverOptions => Energy.ReturnableOptions.Union( CardPlays.ReturnableOptions );

	IEnumerable<Track> Revealed => Energy.Revealed.Union(CardPlays.Revealed );

	public void AdjustEnergyTrackDueToBargain( int delta ) {
		if(delta == 0) return;
		foreach(Track t in Energy.Slots)
			if(t.Energy.HasValue)
				t.Energy += delta;

		InitEnergyAndCardPlays();
	}

	// ------------------------------
	// ----  Reveal / Return  -------
	// ------------------------------

	/// <summary>
	/// Adds Element and updates Energy/CardPlays per turn.
	/// </summary>
	async Task OnTrackRevealedAsync( TrackRevealedArgs args ) {
		var track = args.Track;		
		EnergyPerTurn   = Math.Max( EnergyPerTurn, track.Energy ?? 0 );
		CardPlayPerTurn = Math.Max( CardPlayPerTurn, track.CardPlay ?? 0 );
		Self.Elements.Add(track.Elements);
		if( track.OnRevealAsync is not null)
			await track.OnRevealAsync(track,Self);
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
	virtual public bool CanBePlacedOn( Space space ) => ActionScope.Current.TerrainMapper.IsInPlay( space );

	virtual public bool IsSacredSite( Space space ) => 2 <= CountOn(space) || Self.Mods.OfType<ICreateSacredSites>().Any(mod=>mod.IsSacredSite(space));

	public bool IsOn( Space space ) => 0 < space[Token] || HasIncarna(space);

	// virtual public bool IsOn( Board board ) => Token.IsOn(board);
	public bool IsOn( Board board ) => Token.IsOn(board) 
		|| Incarna.IsPlaced && Incarna.Space.SpaceSpec.Boards.Contains(board);

	public bool IsOnIsland => Token.IsOnIsland || Incarna.IsPlaced;

	public int CountOn( Space space ) => space[Token] // For Mapper in a .Select(...)
		+ (HasIncarna(space) ? 1 : 0);

	public IEnumerable<IToken> TokensDeployedOn( Space space ) {
		// !!! TODO Replace calls to this with:     space.OfTag(this);
		if(0 < space[Token]) yield return Token;
		if(0 < space[Incarna]) yield return Incarna;
	}


	bool HasIncarna(Space ss) => ss == Incarna.Space;

	#endregion

	#region Exposed Data

	/// <summary> Existing </summary>
	/// <remarks> Determining SS requires Tokens so default type for SS is Space. </remarks>
	public IEnumerable<Space> SacredSites      => Lands.Where( IsSacredSite );
	public IEnumerable<Space> SuperSacredSites => Lands.Where( space => 3 <= CountOn( space ) );


	/// <summary> Existing Spaces </summary>
	/// <remarks> Determining presence locations does NOT require Tokens so default type is Space. </remarks>
	public IEnumerable<Space> Lands => Incarna.IsPlaced 
		? new HashSet<Space>([.. Token.Spaces_Existing, Incarna.Space]).OrderBy(x=>x.Label)
		: Token.Spaces_Existing;


	public IEnumerable<SpaceToken> Deployed => Incarna.IsPlaced 
		? [..Token.Deployed, Incarna.AsSpaceToken() ]
		: Token.Deployed;

	public IEnumerable<SpaceToken> Movable => Deployed;

	/// <summary> Unfiltered </summary>
	public int TotalOnIsland() => ActionScope.Current.Spaces_Unfiltered.Sum( CountOn );

	#endregion Exposed Data

	#region ITokenClass Imp
	
	string ITag.Label => "Presence";
	//bool ITokenClass.HasTag( ITag tag ) => tag == this // Spirit.Presence acts like the class for the Spirit
	//	|| tag == TokenCategory.Presence; // !! Should be on Incarna Also
	#endregion ITokenClass Imp

	#region Memento

	public object Memento {
		get => new MyMemento( this );
		set => ((MyMemento)value).Restore( this );
	}

	protected class MyMemento( SpiritPresence _src ) {
		virtual public void Restore( SpiritPresence src ) {
			src.Energy.Memento = _energyTrack;
			src.CardPlays.Memento = _cardPlaysTrack;
			src.Destroyed.Count = _destroyed;
			src.Incarna.Empowered = _incarnaEmpowered;
			// don't need to restore Space because that gets set via Tokens and ITrackMySpaces
			src.AdjustEnergyTrackDueToBargain( _lowestTrackEnergy /* what it should be */ - FirstEnergyTrackValue(src) /* what it is */ );
			src.InitEnergyAndCardPlays(); // force this so Card Plays and energy can re-sync
			src.CustomMementoValue = _tag;
		}

		static int FirstEnergyTrackValue( SpiritPresence src ) => src.Energy.Revealed.First().Energy!.Value; // The first one should always have an energy value.

		readonly object _energyTrack = _src.Energy.Memento;
		readonly object _cardPlaysTrack = _src.CardPlays.Memento;
		readonly object? _tag = _src.CustomMementoValue;
		readonly int _destroyed = _src.Destroyed.Count;
		readonly int _lowestTrackEnergy = FirstEnergyTrackValue( _src );
		readonly bool _incarnaEmpowered = _src.Incarna.Empowered;
	}

	protected virtual object? CustomMementoValue {
		get { return null; }
		set { }
	}

	#endregion

	#region Json

	/// <summary>
	/// [ energyJson, cardPlaysJson, destroyedCount, incarnaEmpowered, customStateJson ]. Only the mutable
	/// runtime deltas - Track structure/slot contents (which Tracks exist on Energy/CardPlays) is
	/// spirit-type data, identical every time the same concrete Spirit subtype is (re)constructed, so it
	/// isn't captured here - same reasoning Spirit.ToJson uses for GrowthTrack's Groups/PickGroups
	/// structure (InnatePowers too; GrowthTrack.Used is the one exception - see
	/// docs/GameSerialization-Roadmap.md's Spirit-core-state section). Energy/CardPlays each serialize
	/// themselves via IPresenceTrack.ToJson/RestoreFromJson - handles CompoundPresenceTrack (Starlight
	/// Seeks Its Form) and FinderTrack (Finder of Paths Unseen) as well as the plain PresenceTrack case.
	/// customStateJson is CustomStateToJson below - SerpentPresence's AbsorbedPresences is the one known
	/// override, hence the ISerializationContext parameter neither Energy/CardPlays/Destroyed/Incarna
	/// otherwise need.
	/// </summary>
	public JsonArray ToJson( ISerializationContext ctx ) => new JsonArray(
		Energy.ToJson(), CardPlays.ToJson(), Destroyed.Count, Incarna.Empowered, CustomStateToJson( ctx ) );

	/// <summary> Restores onto an already-constructed SpiritPresence - see Spirit.RestoreFromJson. </summary>
	public void RestoreFromJson( JsonArray json, ISerializationContext ctx ) {
		Energy.RestoreFromJson( json[0]! );
		CardPlays.RestoreFromJson( json[1]! );
		Destroyed.Count = json[2]!.GetValue<int>();
		Incarna.Empowered = json[3]!.GetValue<bool>();
		RestoreCustomStateFromJson( json[4], ctx );
	}

	/// <summary>
	/// Hook for SerpentPresence's AbsorbedPresences (List&lt;Spirit&gt;) - the one known SpiritPresence
	/// subclass with extra state beyond the fields above (previously only tracked via CustomMementoValue's
	/// in-memory-only Memento). No-op by default. Deliberately independent of CustomMementoValue - Mementos
	/// are slated for removal later (same reasoning as Spirit.CustomStateToJson).
	/// </summary>
	protected virtual JsonNode? CustomStateToJson( ISerializationContext ctx ) => null;

	/// <summary> Restore-side counterpart to CustomStateToJson - see its remarks. </summary>
	protected virtual void RestoreCustomStateFromJson( JsonNode? json, ISerializationContext ctx ) { }

	#endregion
}
