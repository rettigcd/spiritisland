using System;
using System.Threading.Tasks;

namespace SpiritIsland {
	public class Dahan {

		readonly GameState gs;

		public Dahan( GameState gs ) {
			this.gs = gs;
		}

		public void Adjust( Space space, int delta = 1 ) {
			count[space] += delta;
		}

		public Task Destroy( Space space, int countToDestroy, Cause source ) {
			countToDestroy = Math.Min( countToDestroy, Count( space ) );
			Adjust( space, -countToDestroy );
			return Destroyed.InvokeAsync( gs, new DahanDestroyedArgs { space = space, count = countToDestroy, Source = source } );
		}

		public Task Move( Space from, Space to, int count = 1 ) {
			Adjust( from, -count );
			Adjust( to, count );
			return Moved.InvokeAsync( gs, new DahanMovedArgs { from = from, to = to, count = count } );
		}

		public int Count( Space space ) { return count[space]; }
		public bool Has( Space space ) => Count( space ) > 0;

		public AsyncEvent<DahanMovedArgs> Moved = new AsyncEvent<DahanMovedArgs>();                    // Thunderspeaker
		public AsyncEvent<DahanDestroyedArgs> Destroyed = new AsyncEvent<DahanDestroyedArgs>();        // Thunderspeaker

		readonly CountDictionary<Space> count = new CountDictionary<Space>();

	}

	public class DahanDestroyedArgs {
		public Space space;
		public int count;
		public Cause Source;
	};

	public class DahanMovedArgs {
		public Space from;
		public Space to;
		public int count;
	};


}
