namespace SpiritIsland;

public class Tokens_ForIsland : IIslandTokenApi {

	public Tokens_ForIsland( GameState gs ) {

		PenaltyHolder = gs;// new HealthPenaltyPerStrifeHolder();

		TokenDefaults = new Dictionary<TokenClass, IVisibleToken> {
			[Human.City]     = new HumanToken( Human.City, PenaltyHolder, 3 ),
			[Human.Town]     = new HumanToken( Human.Town, PenaltyHolder, 2 ),
			[Human.Explorer] = new HumanToken( Human.Explorer, PenaltyHolder, 1 ),
			[Human.Dahan]    = new HumanToken( Human.Dahan, PenaltyHolder, 2 ),
			[Token.Disease]  = (UniqueToken)Token.Disease,
		};

		gs.TimePasses_WholeGame += (_)=>ClearEventHandlers_ForRound();
	}

	#region Configuration

	IVisibleToken IIslandTokenApi.GetDefault( TokenClass tokenClass ) => TokenDefaults[tokenClass];
	public readonly Dictionary<TokenClass, IVisibleToken> TokenDefaults;

	#endregion

	Task ClearEventHandlers_ForRound() {
		TokenMoved.ForRound.Clear();
		Dynamic.ForRound.Clear();
		return Task.CompletedTask;
	}

	public SpaceState GetTokensFor( Space space ) => this[space];

	public SpaceState this[Space space] {
		get {
			if(!_tokenCounts.ContainsKey( space )) {
				_tokenCounts[space] = new SpaceState( space, new CountDictionary<IToken>(), this );
			}
			return _tokenCounts[space];
		}
	}

	public int GetDynamicTokensFor( SpaceState space, UniqueToken token ) 
		=> Dynamic.GetTokensFor( space, token );

	public async Task Publish_Moved( TokenMovedArgs args ) {
		await TokenMoved.InvokeAsync( args );
	}


	public int InvaderAttack( HumanTokenClass tokenClass ) => Attack[tokenClass];
	public readonly Dictionary<HumanTokenClass, int> Attack = new Dictionary<HumanTokenClass, int> {
		[Human.Explorer] = 1,
		[Human.Town] = 2,
		[Human.City] = 3,
	};

	public readonly DualAsyncEvent<ITokenMovedArgs> TokenMoved = new DualAsyncEvent<ITokenMovedArgs>();

	public readonly DualDynamicTokens Dynamic = new DualDynamicTokens();

	public IEnumerable<SpaceState> PowerUp( IEnumerable<Space> spaces ) => spaces.Select( s => this[s] );

	#region Memento

	public virtual IMemento<Tokens_ForIsland> SaveToMemento() => new Memento(this);
	public virtual void LoadFrom( IMemento<Tokens_ForIsland> memento ) { 
		((Memento)memento).Restore(this);
		ClearEventHandlers_ForRound();
	}

	#region ITrackMySpaces

	// This should ONLY be called from SpaceState.Adjust so that tokens SpaceState & this stay in sync.
	void IIslandTokenApi.Adjust( ITrackMySpaces token, Space space, int delta ) {
		if(!_boardCounts.ContainsKey(token)) _boardCounts.Add(token,new CountDictionary<Board>());
		_boardCounts[token][space.Board] += delta;
	}

	public bool IsOn( ITrackMySpaces token, Board board )
		=> _boardCounts.ContainsKey( token ) && 0 < _boardCounts[token][board];

	readonly Dictionary<ITrackMySpaces,CountDictionary<Board>> _boardCounts = new Dictionary<ITrackMySpaces, CountDictionary<Board>>();

	#endregion

	protected class Memento : IMemento<Tokens_ForIsland> {
		public Memento(Tokens_ForIsland src) {
			// Save TokenCounts
			foreach(var (space,countsDict) in src._tokenCounts.Select( x => ((Space)x.Key, x.Value._counts) ))
				_tokenCounts[space] = countsDict.Clone();
			// Save Defaults
			tokenDefaults = src.TokenDefaults.ToDictionary(p=>p.Key,p=>p.Value);
			// dynamicTokens_ForGame
			dynamicTokens = src.Dynamic.SaveToMemento();
			_inStasis = src._tokenCounts.Keys.ToDictionary(s=>s,s=>s.InStasis);
		}
		public void Restore( Tokens_ForIsland src ) {
			// Restore TokenCounts
			src._tokenCounts.Clear();
			src._boardCounts.Clear();
			var tokenApi = (IIslandTokenApi)src;
			foreach(var space in _tokenCounts.Keys) {
				// stasis
				space.InStasis = _inStasis[space];

				// Token counts
				SpaceState tokens = src[space];
				foreach(var (token,count) in _tokenCounts[space].Select(x=>(x.Key,x.Value))) {
					tokens.Init(token, count);
					tokens.LinkedViaWays = token is GatewayToken gt 
						? gt.To // reapply
						: null; // make sure we clear ones that are no longer linked
					if(tokens is ITrackMySpaces tms)
						tokenApi.Adjust(tms,space,count);
				}
			}
			// Restore Defaults
			src.TokenDefaults.Clear();
			foreach(var pair in tokenDefaults)
				src.TokenDefaults.Add(pair.Key,pair.Value);
			// Restore Dynamic tokens
			src.Dynamic.LoadFrom( dynamicTokens );
		}
		readonly Dictionary<Space, CountDictionary<IToken>> _tokenCounts = new Dictionary<Space, CountDictionary<IToken>>();
		readonly Dictionary<Space, bool> _inStasis;
		readonly Dictionary<TokenClass, IVisibleToken> tokenDefaults = new Dictionary<TokenClass, IVisibleToken>();
		readonly IMemento<DualDynamicTokens> dynamicTokens;
	}

	#endregion Memento

	readonly public IHaveHealthPenaltyPerStrife PenaltyHolder;
	readonly Dictionary<Space, SpaceState> _tokenCounts = new Dictionary<Space, SpaceState>();

}
