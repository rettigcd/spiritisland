using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

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

		public IEnumerable<TokenCountDictionary> ForAllSpaces => tokenCounts.Values;

		readonly Dictionary<Space, TokenCountDictionary> tokenCounts = new Dictionary<Space, TokenCountDictionary>();

		readonly Dictionary<UniqueToken, List<Func<GameState, Space, int>>> dynamicTokens_ForGame = new Dictionary<UniqueToken, List<Func<GameState, Space, int>>>(); // !!! save to memento???
		readonly Dictionary<UniqueToken, List<Func<GameState, Space, int>>> dynamicTokens_ForRound = new Dictionary<UniqueToken, List<Func<GameState, Space, int>>>(); // !!! save to memento???

		public void RegisterDynamic( System.Func<GameState,Space,int> inner, UniqueToken targetToken, bool entireGame ) {
			var dict = entireGame ? dynamicTokens_ForGame : dynamicTokens_ForRound;
			if( !dict.ContainsKey( targetToken ) )
				dict.Add( targetToken, new List<Func<GameState,Space,int>>() );
			dict[targetToken].Add( inner );
		}

		public int GetDynamicTokenFor( Space space, UniqueToken token ) 
			=> GetDynamicDefendFor( dynamicTokens_ForGame, space, token )
			+ GetDynamicDefendFor( dynamicTokens_ForRound, space, token );

		int GetDynamicDefendFor( Dictionary<UniqueToken, List<Func<GameState, Space, int>>> dict, Space space, UniqueToken token ) 
			=> dict.ContainsKey(token) ? dict[token].Sum(x => x( gameStateForEventArgs, space ) ) : 0;


		public IEnumerable<Space> Keys => tokenCounts.Keys;

		public Task Publish_Added( Space space, Token token, int count, AddReason reason ) {
			return TokenAdded.InvokeAsync( gameStateForEventArgs, new TokenAddedArgs(space,token,reason, count) );
		}

		public Task Publish_Removed( Space space, Token token, int count, RemoveReason reason ) {
			return TokenRemoved.InvokeAsync( gameStateForEventArgs, new TokenRemovedArgs( token, reason ) {
				Space = space,
				Count = count,
			} );
		}

		public async Task Publish_Moved( Token token, Space from, Space to ) {
			var args = new TokenMovedArgs {
				Token = token,
				RemovedFrom = from,
				AddedTo = to,
				Count = 1
			};

			await TokenMoved.InvokeAsync( gameStateForEventArgs, args );
			// Also trigger the Added & Removed events
			await TokenAdded.InvokeAsync( gameStateForEventArgs, args );
			await TokenRemoved.InvokeAsync( gameStateForEventArgs, args );
		}

		public override string ToString() {
			return tokenCounts.Keys
				.OrderBy(space=>space.Label)
				.Select(space => this[space].ToString()+"; " )
				.Join("\r\n");
		}

		public DualAsyncEvent<ITokenAddedArgs> TokenAdded = new DualAsyncEvent<ITokenAddedArgs>();
		public DualAsyncEvent<ITokenRemovedArgs> TokenRemoved = new DualAsyncEvent<ITokenRemovedArgs>();
		public DualAsyncEvent<TokenMovedArgs> TokenMoved = new DualAsyncEvent<TokenMovedArgs>();

		#region Memento

		public virtual IMemento<Tokens_ForIsland> SaveToMemento() => new Memento(this);
		public virtual void LoadFrom( IMemento<Tokens_ForIsland> memento ) => ((Memento)memento).Restore(this);
		public TokenCountDictionary GetTokensFor( Space space ) => this[space];

		protected class Memento : IMemento<Tokens_ForIsland> {
			public Memento(Tokens_ForIsland src) {
				foreach(var pair in src.tokenCounts)
					tc[pair.Key] = pair.Value.counts.ToDictionary(p=>p.Key,p=>p.Value);
			}
			public void Restore(Tokens_ForIsland src ) {
				src.tokenCounts.Clear();
				foreach(var space in tc.Keys) {
					var tokens = src[space];
					foreach(var p in tc[space])
						tokens.Init(p.Key, p.Value);
				}
			}
			readonly Dictionary<Space, Dictionary<Token,int>> tc = new Dictionary<Space, Dictionary<Token,int>>();
		}

		#endregion Memento

	}

	#region Event Args Impl

	class TokenAddedArgs : ITokenAddedArgs {

		public TokenAddedArgs(Space space, Token token, AddReason addReason, int count) {
			Space = space;
			Token = token;
			Reason = addReason;
			Count = count;
		}

		public Space Space { get; }
		public Token Token { get; } // need specific so we can act on it (push/damage/destroy)
		public int Count { get; }

		public AddReason Reason { get; }
	}

	class TokenRemovedArgs : ITokenRemovedArgs {
		public TokenRemovedArgs(Token token, RemoveReason reason) { 
			this.Token = token;
			this.Reason = reason;
		}

		public Token Token { get; }
		public int Count { get; set; }
		public Space Space { get; set;}
		public RemoveReason Reason { get; }
	};


	#endregion


}
