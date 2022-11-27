namespace SpiritIsland;

public class Tokens_ForIsland : IIslandTokenApi {

	readonly GameState gameStateForEventArgs; // !!! this is only captured so it can be supplied with the events.

	public Tokens_ForIsland( GameState gs ) {
		this.gameStateForEventArgs = gs;

		PenaltyHolder = gs;// new HealthPenaltyPerStrifeHolder();
		TokenDefaults = new Dictionary<HealthTokenClass, HealthToken> {
			[Invader.City] = new HealthToken( Invader.City, PenaltyHolder, 3 ),
			[Invader.Town] = new HealthToken( Invader.Town, PenaltyHolder, 2 ),
			[Invader.Explorer] = new HealthToken( Invader.Explorer, PenaltyHolder, 1 ),
			[TokenType.Dahan] = new HealthToken( TokenType.Dahan, PenaltyHolder, 2 ),
		};

		gs.TimePasses_WholeGame += (_)=>ClearEventHandlers_ForRound();
	}

	void ClearEventHandlers_ForRound() {
		TokenAdded.ForRound.Clear();
		TokenMoved.ForRound.Clear();
		TokenRemoved.ForRound.Clear();
		RemovingToken.ForRound.Clear();
		Dynamic.ForRound.Clear();
	}

	// !!! every call to this needs checked !.IsInStasis
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
		=> Dynamic.GetTokensFor( gameStateForEventArgs, space, token );

	public IEnumerable<Space> Keys => tokenCounts.Keys;

	public Task Publish_Removing( RemovingTokenArgs args ) => RemovingToken.InvokeAsync( args );

	public Task Publish_Adding( AddingTokenArgs args ) => AddingToken.InvokeAsync( args );

	public Task Publish_Added( TokenAddedArgs args ) {
		args.GameState = gameStateForEventArgs;
		return TokenAdded.InvokeAsync( args );
	}

	public Task Publish_Removed( TokenRemovedArgs args ) {
		args.GameState = gameStateForEventArgs;
		return TokenRemoved.InvokeAsync( args );
	}

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
	public readonly DualAsyncEvent<RemovingTokenArgs> RemovingToken = new DualAsyncEvent<RemovingTokenArgs>();
	public readonly DualAsyncEvent<AddingTokenArgs> AddingToken = new DualAsyncEvent<AddingTokenArgs>();

	public readonly DualAsyncEvent<ITokenAddedArgs> TokenAdded = new DualAsyncEvent<ITokenAddedArgs>();
	public readonly DualAsyncEvent<ITokenRemovedArgs> TokenRemoved = new DualAsyncEvent<ITokenRemovedArgs>();
	public readonly DualAsyncEvent<ITokenMovedArgs> TokenMoved = new DualAsyncEvent<ITokenMovedArgs>();

	public readonly DualDynamicTokens Dynamic = new DualDynamicTokens();

	#region Memento

	public virtual IMemento<Tokens_ForIsland> SaveToMemento() => new Memento(this);
	public virtual void LoadFrom( IMemento<Tokens_ForIsland> memento ) { 
		((Memento)memento).Restore(this);
		ClearEventHandlers_ForRound();
	}
	public SpaceState GetTokensFor( Space space ) => this[space];
	public IEnumerable<SpaceState> PowerUp( IEnumerable<Space> spaces ) => spaces
			.Select( s => this[s] )
			.Where( s => !s.InStasis );

	protected class Memento : IMemento<Tokens_ForIsland> {
		public Memento(Tokens_ForIsland src) {
			// Save TokenCounts
			foreach(var pair in src.tokenCounts)
				tc[pair.Key] = pair.Value.counts.ToDictionary(p=>p.Key,p=>p.Value);
			// Save Defaults
			tokenDefaults = src.TokenDefaults.ToDictionary(p=>p.Key,p=>p.Value);
			// dynamicTokens_ForGame
			dynamicTokens = src.Dynamic.SaveToMemento();
		}
		public void Restore(Tokens_ForIsland src ) {
			// Restore TokenCounts
			src.tokenCounts.Clear();
			foreach(var space in tc.Keys) {
				var tokens = src[space];
				foreach(var p in tc[space]) {
					tokens.Init(p.Key, p.Value);
					tokens.LinkedViaWays = p.Key is GatewayToken gt 
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
		readonly Dictionary<Space, Dictionary<Token,int>> tc = new Dictionary<Space, Dictionary<Token,int>>();
		readonly Dictionary<HealthTokenClass, HealthToken> tokenDefaults = new Dictionary<HealthTokenClass, HealthToken>();
		readonly IMemento<DualDynamicTokens> dynamicTokens;

	}

	#endregion Memento

}

public class DynamicTokens {
	readonly Dictionary<UniqueToken, List<Func<SpaceState, int>>> dict = new Dictionary<UniqueToken, List<Func<SpaceState, int>>>(); // !!! save to memento???
	public void Register( System.Func<SpaceState, int> calcCountOnSpace, UniqueToken targetToken ) {
		if(!dict.ContainsKey( targetToken ))
			dict.Add( targetToken, new List<Func<SpaceState, int>>() );
		dict[targetToken].Add( calcCountOnSpace );
	}
	public int GetDynamicTokenFor( GameState gs, SpaceState space, UniqueToken token )
		=> dict.ContainsKey( token ) ? dict[token].Sum( x => x( space ) ) : 0;
	public void Clear() => dict.Clear();

	public virtual IMemento<DynamicTokens> SaveToMemento() => new Memento( this );
	public virtual void LoadFrom( IMemento<DynamicTokens> memento ) {
		((Memento)memento).Restore( this );
	}

	protected class Memento : IMemento<DynamicTokens> {
		public Memento( DynamicTokens src ) {
			dict = src.dict.ToDictionary(p=>p.Key,p=>p.Value); // make copy
		}
		public void Restore( DynamicTokens src ) {
			src.dict.Clear();
			foreach(var p in dict)
				src.dict.Add(p.Key,p.Value);
		}
		readonly Dictionary<UniqueToken, List<Func<SpaceState, int>>> dict = new Dictionary<UniqueToken, List<Func<SpaceState, int>>>(); // !!! save to memento???
	}


}

public class DualDynamicTokens {
	readonly public DynamicTokens ForGame = new DynamicTokens();
	readonly public DynamicTokens ForRound = new DynamicTokens();
	public void RegisterDynamic( System.Func<SpaceState, int> calcCountOnSpace, UniqueToken targetToken, bool entireGame ) {
		var dTokens = entireGame ? ForGame : ForRound;
		dTokens.Register( calcCountOnSpace, targetToken );
	}
	public int GetTokensFor( GameState gs, SpaceState space, UniqueToken token )
		=> ForGame.GetDynamicTokenFor( gs, space, token )
		+ ForRound.GetDynamicTokenFor( gs, space, token );

	public IMemento<DualDynamicTokens> SaveToMemento() => new Memento( this );
	public void LoadFrom( IMemento<DualDynamicTokens> memento ) {
		((Memento)memento).Restore( this );
	}

	protected class Memento : IMemento<DualDynamicTokens> {
		public Memento( DualDynamicTokens src ) {
			forGame = src.ForGame.SaveToMemento();
		}
		public void Restore( DualDynamicTokens src ) {
			src.ForGame.LoadFrom( forGame );
			src.ForRound.Clear();
		}
		readonly IMemento<DynamicTokens> forGame;
	}

}

#region Event Args Impl

#endregion
