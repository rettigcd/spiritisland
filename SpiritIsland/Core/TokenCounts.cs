using System.Threading.Tasks;


namespace SpiritIsland {

	public class TokenCounts {

		readonly GameState gameState;

		public TokenCounts(GameState gameState ) {
			this.gameState = gameState;
		}

		public bool AreOn( Space space ) => count[space] > 0;

		public int GetCount( Space s ) => count[s];

		public Task Move( Space from, Space to, int count = 1 ) {
			this.count[from] -= count;
			this.count[to] += count;
			return Moved.InvokeAsync( gameState, new TokenMovedArgs { from = from, to = to, count = count } );
		}

		public AsyncEvent<TokenMovedArgs> Moved = new AsyncEvent<TokenMovedArgs>();                    // Thunderspeaker

		readonly CountDictionary<Space> count = new CountDictionary<Space>();

		#region adjust

		public void AddOneTo( Space space ) { count[space]++; }

		public void RemoveOneFrom( Space space ) { count[space]--; }

		#endregion


	}

}
