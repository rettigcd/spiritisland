using System;
using System.Threading.Tasks;

namespace SpiritIsland {

	// ! with a little work, we could make this inherit from TokenCounts
	public class Dahan {

		readonly GameState gameState;

		public Dahan( GameState gs ) {
			this.gameState = gs;
		}

		public Task Destroy( Space space, int countToDestroy, Cause source ) {
			countToDestroy = Math.Min( countToDestroy, gameState.DahanGetCount( space ) );
			gameState.DahanAdjust( space, -countToDestroy );
			return gameState.Tokens.TokenDestroyed.InvokeAsync( gameState, new TokenDestroyedArgs { 
				Token = TokenType.Dahan,
				space = space, 
				count = countToDestroy, 
				Source = source 
			} );
		}

	}


}
