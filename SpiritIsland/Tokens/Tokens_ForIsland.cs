namespace SpiritIsland;

public class Tokens_ForIsland : IIslandTokenApi {

	readonly GameState gameStateForEventArgs; // !!! this is only captured so it can be supplied with the events.

	public Tokens_ForIsland( GameState gs ) {
		this.gameStateForEventArgs = gs;

		PenaltyHolder = gs;// new HealthPenaltyPerStrifeHolder();
		TokenDefaults = new Dictionary<HealthTokenClass, HealthToken> {
			[Invader.City]     = new HealthToken( Invader.City, PenaltyHolder, 3 ),
			[Invader.Town]     = new HealthToken( Invader.Town, PenaltyHolder, 2 ),
			[Invader.Explorer] = new HealthToken( Invader.Explorer, PenaltyHolder, 1 ),
			[TokenType.Dahan]  = new HealthToken( TokenType.Dahan, PenaltyHolder, 2 ),
		};

		gs.TimePasses_WholeGame += (_)=>ClearEventHandlers_ForRound();
	}

	Task ClearEventHandlers_ForRound() {
		TokenAdded.ForRound.Clear();
		TokenMoved.ForRound.Clear();
		TokenRemoved.ForRound.Clear();
		Dynamic.ForRound.Clear();
		return Task.CompletedTask;
	}

	public SpaceState this[Space space] {
		get {
			if(!tokenCounts.ContainsKey( space )) {
				tokenCounts[space] = new SpaceState( space, new CountDictionary<Token>(), this, this.gameStateForEventArgs );
			}
			return tokenCounts[space];
		}
	}

	readonly Dictionary<Space, SpaceState> tokenCounts = new Dictionary<Space, SpaceState>();

	public int GetDynamicTokensFor( SpaceState space, UniqueToken token ) 
		=> Dynamic.GetTokensFor( space, token );

	public IEnumerable<Space> Keys => tokenCounts.Keys;

	public Task Publish_Adding( AddingTokenArgs args ) => AddingToken.InvokeAsync( args );

	public Task Publish_Added( TokenAddedArgs args ) {
		args.GameState = gameStateForEventArgs;
		return TokenAdded.InvokeAsync( args );
	}

	public Task Publish_Removed( PublishTokenRemovedArgs args ) => TokenRemoved.InvokeAsync( args.MakeEvent(gameStateForEventArgs) );

	public async Task Publish_Moved( TokenMovedArgs args ) {
		args.GameState=this.gameStateForEventArgs;
		await TokenMoved.InvokeAsync( args );
	}

	public override string ToString() {
		return tokenCounts.Keys
			.OrderBy(space=>space.Label)
			.Select(space => this[space].ToString()+"; " )
			.Join("\r\n");
	}

	readonly public IHaveHealthPenaltyPerStrife PenaltyHolder;
	readonly public Dictionary<HealthTokenClass, HealthToken> TokenDefaults;

	HealthToken IIslandTokenApi.GetDefault( HealthTokenClass tokenClass ) => TokenDefaults[tokenClass];

	/// <summary> Sent before any token is removed. </summary>
	/// <remarks> Callers may modify the args to disable the remove if desired. </remarks>
	public readonly DualAsyncEvent<AddingTokenArgs> AddingToken = new DualAsyncEvent<AddingTokenArgs>();

	public readonly DualAsyncEvent<ITokenAddedArgs> TokenAdded = new DualAsyncEvent<ITokenAddedArgs>();
	public readonly DualAsyncEvent<ITokenRemovedArgs> TokenRemoved = new DualAsyncEvent<ITokenRemovedArgs>();
	public readonly DualAsyncEvent<ITokenMovedArgs> TokenMoved = new DualAsyncEvent<ITokenMovedArgs>();

	public readonly DualDynamicTokens Dynamic = new DualDynamicTokens();

	public SpaceState GetTokensFor( Space space ) => this[space];
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
			foreach(var (space,countsDict) in src.tokenCounts.Select( x => (x.Key, x.Value.counts) ))
				_tokenCounts[space] = countsDict.Clone();
			// Save Defaults
			tokenDefaults = src.TokenDefaults.ToDictionary(p=>p.Key,p=>p.Value);
			// dynamicTokens_ForGame
			dynamicTokens = src.Dynamic.SaveToMemento();
			_inStasis = src.tokenCounts.Keys.ToDictionary(s=>s,s=>s.InStasis);
		}
		public void Restore( Tokens_ForIsland src ) {
			// Restore TokenCounts
			src.tokenCounts.Clear();
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
		readonly Dictionary<Space, CountDictionary<Token>> _tokenCounts = new Dictionary<Space, CountDictionary<Token>>();
		readonly Dictionary<Space, bool> _inStasis;
		readonly Dictionary<HealthTokenClass, HealthToken> tokenDefaults = new Dictionary<HealthTokenClass, HealthToken>();
		readonly IMemento<DualDynamicTokens> dynamicTokens;
	}

	#endregion Memento

}
