using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class IslandTokens {

		readonly GameState gs;
		public IslandTokens( GameState gs ) {
			this.gs = gs;

			gs.TimePassed += TokenMoved.EndOfRound;
			gs.TimePassed += TokenDestroyed.EndOfRound;
		}

		public TokenCountDictionary this[Space space] {
			get {
				var countArray = invaderCount.ContainsKey( space )
					? invaderCount[space]
					: invaderCount[space] = new CountDictionary<Token>();
				return new TokenCountDictionary( space, countArray );
			}
		}

		public IEnumerable<Space> Keys => invaderCount.Keys;

		readonly Dictionary<Space, CountDictionary<Token>> invaderCount = new Dictionary<Space, CountDictionary<Token>>();

		public Task Move( Token token, Space from, Space to, int count = 1 ) {
			this[ from ].Adjust( token, -count );
			this[ to ].Adjust( token, count );
			return TokenMoved.InvokeAsync( gs, new TokenMovedArgs {
				Token = token,
				from = from,
				to = to,
				count = count
			} );
		}

		public AsyncEvent<TokenMovedArgs> TokenMoved = new AsyncEvent<TokenMovedArgs>();
		public AsyncEvent<TokenDestroyedArgs> TokenDestroyed = new AsyncEvent<TokenDestroyedArgs>();

	}

}
