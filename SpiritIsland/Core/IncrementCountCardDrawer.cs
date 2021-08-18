using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class IncrementCountCardDrawer : IPowerCardDrawer {
		public Task<PowerCard> Draw( ActionEngine engine, Func<List<PowerCard>, Task> _ ) {
			engine.Self.PowerCardsToDraw++;
			return Task.FromResult<PowerCard>( null );
		}

		public Task<PowerCard> DrawMajor( ActionEngine engine, Func<List<PowerCard>, Task> _ ) {
			engine.Self.PowerCardsToDraw++;
			return Task.FromResult<PowerCard>( null );
		}

		public Task<PowerCard> DrawMinor( ActionEngine engine, Func<List<PowerCard>, Task> _ ) {
			engine.Self.PowerCardsToDraw++;
			return Task.FromResult<PowerCard>(null);
		}

	}


}