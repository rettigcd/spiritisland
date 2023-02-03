namespace SpiritIsland;

public class Tokens_ForIsland : IIslandTokenApi {

	readonly GameState _gameState; // captures so we can give it to SpaceState

	public Tokens_ForIsland( GameState gs ) {
		_gameState = gs;

		PenaltyHolder = gs;// new HealthPenaltyPerStrifeHolder();
		TokenDefaults = new Dictionary<HumanTokenClass, HumanToken> {
			[Human.City]     = new HumanToken( Human.City, PenaltyHolder, 3 ),
			[Human.Town]     = new HumanToken( Human.Town, PenaltyHolder, 2 ),
			[Human.Explorer] = new HumanToken( Human.Explorer, PenaltyHolder, 1 ),
			[Human.Dahan]  = new HumanToken( Human.Dahan, PenaltyHolder, 2 ),
		};

		gs.TimePasses_WholeGame += (_)=>ClearEventHandlers_ForRound();
	}

	Task ClearEventHandlers_ForRound() {
		TokenMoved.ForRound.Clear();
		Dynamic.ForRound.Clear();
		return Task.CompletedTask;
	}

	public SpaceState GetTokensFor( Space space ) => this[space];

	public SpaceState this[Space space] {
		get {
			if(!_tokenCounts.ContainsKey( space )) {
				_tokenCounts[space] = new SpaceState( space, new CountDictionary<IToken>(), this, _gameState );
			}
			return _tokenCounts[space];
		}
	}

	public int GetDynamicTokensFor( SpaceState space, UniqueToken token ) 
		=> Dynamic.GetTokensFor( space, token );

	public async Task Publish_Moved( TokenMovedArgs args ) {
		await TokenMoved.InvokeAsync( args );
	}

	HumanToken IIslandTokenApi.GetDefault( HumanTokenClass tokenClass ) => TokenDefaults[tokenClass];

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
			foreach(var space in _tokenCounts.Keys) {
				// statis
				space.InStasis = _inStasis[space];

				// Token counts
				SpaceState tokens = src[space];
				foreach(var (token,count) in _tokenCounts[space].Select(x=>(x.Key,x.Value))) {
					tokens.Init(token, count);
					tokens.LinkedViaWays = token is GatewayToken gt 
						? gt.To // reapply
						: null; // make sure we clear ones that are no longer linked
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
		readonly Dictionary<HumanTokenClass, HumanToken> tokenDefaults = new Dictionary<HumanTokenClass, HumanToken>();
		readonly IMemento<DualDynamicTokens> dynamicTokens;
	}

	#endregion Memento

	readonly public IHaveHealthPenaltyPerStrife PenaltyHolder;
	readonly public Dictionary<HumanTokenClass, HumanToken> TokenDefaults;
	readonly Dictionary<Space, SpaceState> _tokenCounts = new Dictionary<Space, SpaceState>();

}
