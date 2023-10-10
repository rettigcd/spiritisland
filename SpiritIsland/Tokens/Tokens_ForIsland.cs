namespace SpiritIsland;

public class Tokens_ForIsland : IIslandTokenApi {

	public Tokens_ForIsland( GameState gs ) {

		PenaltyHolder = gs;// new HealthPenaltyPerStrifeHolder();

		TokenDefaults = new Dictionary<IEntityClass, IToken> {
			[Human.City]     = new HumanToken( Human.City, PenaltyHolder, 3 ),
			[Human.Town]     = new HumanToken( Human.Town, PenaltyHolder, 2 ),
			[Human.Explorer] = new HumanToken( Human.Explorer, PenaltyHolder, 1 ),
			[Human.Dahan]    = new HumanToken( Human.Dahan, PenaltyHolder, 2 ),
			[Token.Disease]  = Token.Disease,
		};

		_islandMods = new CountDictionary<ISpaceEntity>();
		// stick it in here so it is persisted and cleaned up during time passes
		_tokenCounts.Add( new FakeSpace( "Island-Mods" ), _islandMods );

		gs.TimePasses_WholeGame += (_)=>ClearEventHandlers_ForRound();
	}

	#region Configuration

	IToken IIslandTokenApi.GetDefault( IEntityClass tokenClass ) => TokenDefaults[tokenClass];
	public readonly Dictionary<IEntityClass, IToken> TokenDefaults;

	#endregion

	readonly CountDictionary<ISpaceEntity> _islandMods;
	public void TimePasses() {
		foreach(var pair in _tokenCounts)
			new SpaceState(pair.Key,pair.Value,_islandMods.Keys,this).TimePasses();
	}

	public void AddIslandMod( BaseModEntity token ) { ++_islandMods[token]; }


	Task ClearEventHandlers_ForRound() {
		Dynamic.ForRound.Clear();
		return Task.CompletedTask;
	}

	/// <remarks>
	/// Spirit Actions should not call this directly but rather go through Space.Tokens => ActionScope.AccessTokens()
	/// UI or Test stuff that is outside of an ActionScope, may use this directly.
	/// </remarks>
	public SpaceState this[Space space] => new SpaceState( space, GetTokensCounts( space ), _islandMods.Keys, this );

	CountDictionary<ISpaceEntity> GetTokensCounts( Space key ) => _tokenCounts.Get( key, () => new CountDictionary<ISpaceEntity>() );


	public int GetDynamicTokensFor( SpaceState space, TokenClassToken token ) 
		=> Dynamic.GetTokensFor( space, token );

	public int InvaderAttack( HumanTokenClass tokenClass ) => Attack[tokenClass];
	public readonly Dictionary<HumanTokenClass, int> Attack = new Dictionary<HumanTokenClass, int> {
		[Human.Explorer] = 1,
		[Human.Town] = 2,
		[Human.City] = 3,
	};

	public readonly DualDynamicTokens Dynamic = new DualDynamicTokens();

	#region Memento

	public virtual IMemento<Tokens_ForIsland> SaveToMemento() => new Memento(this);
	public virtual void LoadFrom( IMemento<Tokens_ForIsland> memento ) { 
		((Memento)memento).Restore(this);
		ClearEventHandlers_ForRound();
	}

	#region ITrackMySpaces

	// This should ONLY be called from SpaceState.Adjust so that tokens SpaceState & this stay in sync.
	void IIslandTokenApi.Adjust( ITrackMySpaces token, Space space, int delta ) {
		// Track which boards a token is on (and how many)
		if(!_boardCounts.ContainsKey(token)) _boardCounts.Add(token,new CountDictionary<Board>());
		foreach(Board board in space.Boards)
			_boardCounts[token][board] += delta;
		// Track which Spaces a token is on (and how many)
		if(!_spaceCounts.ContainsKey( token )) _spaceCounts.Add( token, new CountDictionary<Space>() );
		_spaceCounts[token][space] += delta;
	}

	public bool IsOn( ITrackMySpaces token, Board board ) => _boardCounts.ContainsKey( token ) && 0 < _boardCounts[token][board];

	/// <returns>non-stasis spaces containing the (ITrackMySpaces) token.</returns>
	/// <remarks>used for finding presence</remarks>
	public IEnumerable<Space> Spaces_Existing( ITrackMySpaces token ) => _spaceCounts.ContainsKey( token ) 
		? _spaceCounts[token].Keys.Where( Space.Exists ) 
		: Enumerable.Empty<Space>();

	/// <summary> Determines if the token is on any board. </summary>
	public bool IsOnAnyBoard( ITrackMySpaces token ) => _boardCounts.ContainsKey( token ) && _boardCounts[token].Any();

	readonly Dictionary<ITrackMySpaces,CountDictionary<Board>> _boardCounts = new Dictionary<ITrackMySpaces, CountDictionary<Board>>();
	readonly Dictionary<ITrackMySpaces, CountDictionary<Space>> _spaceCounts = new Dictionary<ITrackMySpaces, CountDictionary<Space>>();

	#endregion

	protected class Memento : IMemento<Tokens_ForIsland> {
		public Memento(Tokens_ForIsland src) {
			// Save TokenCounts
			foreach(var (space,countsDict) in src._tokenCounts.Select( x => (x.Key, x.Value) ))
				_tokenCounts[space] = countsDict.Clone();
			// Save Defaults
			tokenDefaults = src.TokenDefaults.ToDictionary(p=>p.Key,p=>p.Value);
			// dynamicTokens_ForGame
			dynamicTokens = src.Dynamic.SaveToMemento();
			_doesNotExist = src._tokenCounts.Keys.Where(s=>!s.DoesExists).ToArray();
		}
		public void Restore( Tokens_ForIsland src ) {
			// Restore TokenCounts
			src._tokenCounts.Clear();
			src._boardCounts.Clear();
			src._spaceCounts.Clear();
			var tokenApi = (IIslandTokenApi)src;
			foreach(var space in _tokenCounts.Keys) {
				// stasis
				space.DoesExists = true; // when false, set below

				// Token counts
				SpaceState tokens = src[space];
				foreach(var (token,count) in _tokenCounts[space].Select(x=>(x.Key,x.Value))) {
					tokens.Init(token, count);
					if(tokens is ITrackMySpaces tms)
						tokenApi.Adjust(tms,space,count);
				}
			}
			foreach(var space in _doesNotExist) space.DoesExists = false;
			// Restore Defaults
			src.TokenDefaults.Clear();
			foreach(var pair in tokenDefaults)
				src.TokenDefaults.Add(pair.Key,pair.Value);
			// Restore Dynamic tokens
			src.Dynamic.LoadFrom( dynamicTokens );
		}
		readonly Dictionary<Space, CountDictionary<ISpaceEntity>> _tokenCounts = new Dictionary<Space, CountDictionary<ISpaceEntity>>();
		readonly Space[] _doesNotExist;
		readonly Dictionary<IEntityClass, IToken> tokenDefaults = new Dictionary<IEntityClass, IToken>();
		readonly IMemento<DualDynamicTokens> dynamicTokens;
	}

	#endregion Memento

	public override string ToString() => _tokenCounts
		.Select(p=>p.Key+":"+p.Value.TokenSummary())
		.Join(" ");

	public string ToVerboseString() => _tokenCounts
		.OrderBy( p => p.Key.Label )
		.Select( p => p.Key + ":" + p.Value.TokensVerbose() )
		.Join( "\r\n" );

	readonly public IHaveHealthPenaltyPerStrife PenaltyHolder;
	readonly Dictionary<Space, CountDictionary<ISpaceEntity>> _tokenCounts = new Dictionary<Space, CountDictionary<ISpaceEntity>>();

}
