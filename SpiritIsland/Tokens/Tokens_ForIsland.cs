using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class Tokens_ForIsland : IDestroyIslandTokens {

		readonly GameState gameStateForEventArgs; // !!! this is only captured so it can be supplied with the events.

		public Tokens_ForIsland( GameState gs ) {
			this.gameStateForEventArgs = gs;

			gs.TimePasses_WholeGame += TokenAdded.OnEndOfRound;
			gs.TimePasses_WholeGame += TokenMoved.OnEndOfRound;
			gs.TimePasses_WholeGame += TokenDestroyed.OnEndOfRound;
		}

		public TokenCountDictionary this[Space space] {
			get {
				if(!tokenCounts.ContainsKey( space )) {
					tokenCounts[space] = new TokenCountDictionary( space, new CountDictionary<Token>(), this );
				}
				return tokenCounts[space];
			}
		}

//		readonly Dictionary<Space, CountDictionary<Token>> countDict = new Dictionary<Space, CountDictionary<Token>>();
		readonly Dictionary<Space, TokenCountDictionary> tokenCounts = new Dictionary<Space, TokenCountDictionary>();

		public IEnumerable<Space> Keys => tokenCounts.Keys;

		public Task Move( Token token, Space from, Space to ) {
			if( token.Generic == TokenType.Dahan )
				return MoveDahan( token, from, to );
			else {
				this[ from ].Adjust( token, -1 );
				this[ to ].Adjust( token, 1 );
				return TokenMoved.InvokeAsync( gameStateForEventArgs, new TokenMovedArgs {
					Token = token,
					From = from,
					To = to,
					count = 1
				} );
			}
		}

		Task MoveDahan( Token token, Space from, Space to ) {
			Token removedToken = this[from].Dahan.Remove1(token);
			if(removedToken == null) return Task.CompletedTask;

			this[to].Dahan.Add( removedToken );
			return TokenMoved.InvokeAsync( gameStateForEventArgs, new TokenMovedArgs {
				Token = token,
				From = from,
				To = to,
				count = 1
			} );
		}

		// For Build / Explore.
		// Not for gather / push / replace
		public async Task Add( TokenGroup group, Space space, int delta = 1 ) {
			if(delta < 0) throw new System.ArgumentOutOfRangeException( nameof( delta ) );
			var token = group.Default;
			this[space][token] += delta;

			await TokenAdded.InvokeAsync( gameStateForEventArgs, new TokenAddedArgs{ 
				count = delta,
				Space = space,
				Token = token
			} );
		}

		public override string ToString() {
			return tokenCounts.Keys
				.OrderBy(space=>space.Label)
				.Select(space => this[space].ToString()+"; " )
				.Join("\r\n");
		}

		public AsyncEvent<TokenAddedArgs> TokenAdded = new AsyncEvent<TokenAddedArgs>();
		public AsyncEvent<TokenMovedArgs> TokenMoved = new AsyncEvent<TokenMovedArgs>();
		public AsyncEvent<TokenDestroyedArgs> TokenDestroyed = new AsyncEvent<TokenDestroyedArgs>();

		public Task DestroyIslandToken( Space space, int countToDestroy, Token token, Cause source ) {

			if(countToDestroy==0) return Task.CompletedTask;
			var tokens = this[space];
			countToDestroy = System.Math.Min( countToDestroy, tokens[token] );
			if(countToDestroy==0) return Task.CompletedTask;
			tokens[token] -= countToDestroy;

			return TokenDestroyed.InvokeAsync( gameStateForEventArgs, new TokenDestroyedArgs {
				Token = token.Generic,
				Space = space,
				count = countToDestroy,
				Source = source
			} );
		}

		#region Memento

		public virtual IMemento<Tokens_ForIsland> SaveToMemento() => new Memento(this);
		public virtual void LoadFrom( IMemento<Tokens_ForIsland> memento ) => ((Memento)memento).Restore(this);

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
						tokens[p.Key] = p.Value;
				}
			}
			readonly Dictionary<Space, Dictionary<Token,int>> tc = new Dictionary<Space, Dictionary<Token,int>>();
		}

		#endregion Memento

	}

	public interface IDestroyIslandTokens {
		Task DestroyIslandToken( Space space, int countToDestroy, Token token, Cause source );
	}


}
