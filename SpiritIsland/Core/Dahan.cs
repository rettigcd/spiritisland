using System;
using System.Threading.Tasks;

namespace SpiritIsland {

	// ! with a little work, we could make this inherit from TokenCounts
	public class Dahan {

		readonly GameState gameState;

		public Dahan( GameState gs ) {
			this.gameState = gs;
		}

		public bool AreOn( Space space ) => GetCount( space ) > 0;

		public int GetCount( Space space ) => count[space];

		public Task Move( Space from, Space to, int count = 1 ) {
			this.count[from] -= count;
			this.count[to] += count;
			return Moved.InvokeAsync( gameState, new TokenMovedArgs { from = from, to = to, count = count } );
		}

		public AsyncEvent<TokenMovedArgs> Moved = new AsyncEvent<TokenMovedArgs>();                    // Thunderspeaker

		readonly CountDictionary<Space> count = new CountDictionary<Space>();

		#region adjust

		public void Adjust( Space space, int delta = 1 ) {
			count[space] += delta;
		}

		#endregion



		public Task Destroy( Space space, int countToDestroy, Cause source ) {
			countToDestroy = Math.Min( countToDestroy, GetCount( space ) );
			Adjust( space, -countToDestroy );
			return Destroyed.InvokeAsync( gameState, new DahanDestroyedArgs { space = space, count = countToDestroy, Source = source } );
		}

		public AsyncEvent<DahanDestroyedArgs> Destroyed = new AsyncEvent<DahanDestroyedArgs>();        // Thunderspeaker


	}

	public class DahanDestroyedArgs {
		public Space space;
		public int count;
		public Cause Source;
	};

	public class TokenMovedArgs {
		public Space from;
		public Space to;
		public int count;
	};


}
