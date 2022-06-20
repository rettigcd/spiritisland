namespace SpiritIsland;

public class Tokens_ForIsland : IIslandTokenApi {

	readonly GameState gameStateForEventArgs; // !!! this is only captured so it can be supplied with the events.

	public Tokens_ForIsland( GameState gs ) {
		this.gameStateForEventArgs = gs;

		gs.TimePasses_WholeGame += TimePasses;
	}

	void TimePasses( GameState _ ) {
		TokenAdded.ForRound.Clear();
		TokenMoved.ForRound.Clear();
		TokenRemoved.ForRound.Clear();
		RemovingToken.ForRound.Clear();
		dynamicTokens_ForRound.Clear();
	}

	public TokenCountDictionary this[Space space] {
		get {
			if(!tokenCounts.ContainsKey( space )) {
				tokenCounts[space] = new TokenCountDictionary( space, new CountDictionary<Token>(), this );
			}
			return tokenCounts[space];
		}
	}

	public TokenCountDictionary For(Space space) {
		if(!tokenCounts.ContainsKey( space )) {
			tokenCounts[space] = new TokenCountDictionary( space, new CountDictionary<Token>(), this );
		}
		return tokenCounts[space];
	}


	public IEnumerable<TokenCountDictionary> ForAllSpaces => tokenCounts.Values;

	readonly Dictionary<Space, TokenCountDictionary> tokenCounts = new Dictionary<Space, TokenCountDictionary>();

	readonly Dictionary<UniqueToken, List<Func<GameState, Space, int>>> dynamicTokens_ForGame = new Dictionary<UniqueToken, List<Func<GameState, Space, int>>>(); // !!! save to memento???
	readonly Dictionary<UniqueToken, List<Func<GameState, Space, int>>> dynamicTokens_ForRound = new Dictionary<UniqueToken, List<Func<GameState, Space, int>>>();

	public void RegisterDynamic( System.Func<GameState,Space,int> calcCountOnSpace, UniqueToken targetToken, bool entireGame ) {
		var dict = entireGame ? dynamicTokens_ForGame : dynamicTokens_ForRound;
		if( !dict.ContainsKey( targetToken ) )
			dict.Add( targetToken, new List<Func<GameState,Space,int>>() );
		dict[targetToken].Add( calcCountOnSpace );
	}

	public int GetDynamicTokenFor( Space space, UniqueToken token ) 
		=> GetDynamicDefendFor( dynamicTokens_ForGame, space, token )
		+ GetDynamicDefendFor( dynamicTokens_ForRound, space, token );

	int GetDynamicDefendFor( Dictionary<UniqueToken, List<Func<GameState, Space, int>>> dict, Space space, UniqueToken token ) 
		=> dict.ContainsKey(token) ? dict[token].Sum(x => x( gameStateForEventArgs, space ) ) : 0;


	public IEnumerable<Space> Keys => tokenCounts.Keys;

	public Task Publish_Removing( RemovingTokenArgs args ) {
		return RemovingToken.InvokeAsync( args );
	}

	public Task Publish_Added( Space space, Token token, int count, AddReason reason, Guid actionId ) {
		return TokenAdded.InvokeAsync( new TokenAddedArgs(space,token,reason, count, gameStateForEventArgs, actionId) );
	}

	public Task Publish_Removed( TokenRemovedArgs args ) {
		args.GameState = gameStateForEventArgs;
		return TokenRemoved.InvokeAsync( args );
	}

	public async Task Publish_Moved( Token token, Space from, Space to, Guid actionId ) {
		var args = new TokenMovedArgs {
			Token = token,
			RemovedFrom = from,
			AddedTo = to,
			Count = 1,
			GameState = this.gameStateForEventArgs,
			ActionId = actionId
		};

		await TokenMoved.InvokeAsync( args );
		// Also trigger the Added & Removed events
		await TokenAdded.InvokeAsync( args );
		await TokenRemoved.InvokeAsync( args );
	}

	public override string ToString() {
		return tokenCounts.Keys
			.OrderBy(space=>space.Label)
			.Select(space => this[space].ToString()+"; " )
			.Join("\r\n");
	}

	HealthToken IIslandTokenApi.GetDefault( HealthTokenClass tokenClass ) => TokenDefaults[tokenClass];
	public Dictionary<HealthTokenClass,HealthToken> TokenDefaults = new Dictionary<HealthTokenClass, HealthToken> {
		[Invader.City] = new HealthToken( Invader.City, 3 ),
		[Invader.Town] = new HealthToken( Invader.Town, 2 ),
		[Invader.Explorer] = new HealthToken( Invader.Explorer, 1 ),
		[TokenType.Dahan] = new HealthToken( TokenType.Dahan, 2 ),
	};

	/// <summary> Sent before any token is removed. </summary>
	/// <remarks> Callers may modify the args to disable the remove if desired. </remarks>
	public DualAsyncEvent<RemovingTokenArgs> RemovingToken = new DualAsyncEvent<RemovingTokenArgs>();

	public DualAsyncEvent<ITokenAddedArgs> TokenAdded = new DualAsyncEvent<ITokenAddedArgs>();
	public DualAsyncEvent<ITokenRemovedArgs> TokenRemoved = new DualAsyncEvent<ITokenRemovedArgs>();
	public DualAsyncEvent<TokenMovedArgs> TokenMoved = new DualAsyncEvent<TokenMovedArgs>();

	#region Memento

	public virtual IMemento<Tokens_ForIsland> SaveToMemento() => new Memento(this);
	public virtual void LoadFrom( IMemento<Tokens_ForIsland> memento ) => ((Memento)memento).Restore(this);
	public TokenCountDictionary GetTokensFor( Space space ) => this[space];

	protected class Memento : IMemento<Tokens_ForIsland> {
		public Memento(Tokens_ForIsland src) {
			// Save TokenCounts
			foreach(var pair in src.tokenCounts)
				tc[pair.Key] = pair.Value.counts.ToDictionary(p=>p.Key,p=>p.Value);
			// Save Defaults
			tokenDefaults = src.TokenDefaults.ToDictionary(p=>p.Key,p=>p.Value);
		}
		public void Restore(Tokens_ForIsland src ) {
			// Resotre TokenCounts
			src.tokenCounts.Clear();
			foreach(var space in tc.Keys) {
				var tokens = src[space];
				foreach(var p in tc[space])
					tokens.Init(p.Key, p.Value);
			}
			// Restore Defaults
			src.TokenDefaults.Clear();
			foreach(var pair in tokenDefaults)
				src.TokenDefaults.Add(pair.Key,pair.Value);
		}
		readonly Dictionary<Space, Dictionary<Token,int>> tc = new Dictionary<Space, Dictionary<Token,int>>();
		readonly Dictionary<HealthTokenClass, HealthToken> tokenDefaults = new Dictionary<HealthTokenClass, HealthToken>();

		}

	#endregion Memento

}

#region Event Args Impl

class TokenAddedArgs : ITokenAddedArgs {

	public TokenAddedArgs(Space space, Token token, AddReason addReason, int count, GameState gs, Guid actionId ) {
		Space = space;
		Token = token;
		Reason = addReason;
		Count = count;
		GameState = gs;
		ActionId = actionId;
	}

	public GameState GameState { get; }
	public Space Space { get; }
	public Token Token { get; } // need specific so we can act on it (push/damage/destroy)
	public int Count { get; }

	public AddReason Reason { get; }

	public Guid ActionId { get; }

}

public class RemovingTokenArgs {
	public Space Space { get; set; }
	public Token Token { get; set; }
	public int Count {
		get { return _count; }
		set { 
			// !!! something is making this negative
			if(value<0) throw new ArgumentOutOfRangeException(nameof(value),value,"Removing Token Args cannot be < 0");
			_count = value;
		}
	}
	int _count;
	public RemoveReason Reason { get; set; }
	public Guid ActionId { get; set; }
}

public class TokenRemovedArgs : ITokenRemovedArgs {

	public TokenRemovedArgs(Token token, RemoveReason reason, Guid actionId, Space space, int count ) {
		Token = token;
		Reason = reason;
		ActionId = actionId;
		Space = space;
		Count = count;
	}

	public GameState GameState { get; set; }// set by the token-publisher because TokenCountDictionary doesn't have this info

	public Token Token { get; }
	public int Count { get; set; }
	public Space Space { get; set;}
	public RemoveReason Reason { get; }
	public Guid ActionId { get; }
};

#endregion
