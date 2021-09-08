﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class Tokens_ForIsland {

		readonly GameState gs;
		public Tokens_ForIsland( GameState gs ) {
			this.gs = gs;

			gs.TimePassed += TokenAdded.EndOfRound;
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

		// For Build / Explore.
		// Not for gather / push / replace
		public async Task Add( TokenGroup group, Space space, int delta = 1 ) {
			if(delta < 0) throw new System.ArgumentOutOfRangeException( nameof( delta ) );
			var token = group.Default;
			this[space][token] += delta;

			await TokenAdded.InvokeAsync( gs, new TokenAddedArgs{ 
				count = delta,
				Space = space,
				Token = token
			} );
		}


		public AsyncEvent<TokenAddedArgs> TokenAdded = new AsyncEvent<TokenAddedArgs>();
		public AsyncEvent<TokenMovedArgs> TokenMoved = new AsyncEvent<TokenMovedArgs>();
		public AsyncEvent<TokenDestroyedArgs> TokenDestroyed = new AsyncEvent<TokenDestroyedArgs>();

	}

}