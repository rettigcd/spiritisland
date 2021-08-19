using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class IncrementCountCardDrawer : IPowerCardDrawer {

		public Task<PowerCard> Draw( Spirit self, GameState _, Func<List<PowerCard>, Task> _1 ) {
			self.PowerCardsToDraw++;
			return Task.FromResult<PowerCard>( null );
		}

		public Task<PowerCard> DrawMajor( Spirit self, GameState _, Func<List<PowerCard>, Task> _1 ) {
			self.PowerCardsToDraw++;
			return Task.FromResult<PowerCard>( null );
		}

		public Task<PowerCard> DrawMinor( Spirit self, GameState _, Func<List<PowerCard>, Task> _1 ) {
			self.PowerCardsToDraw++;
			return Task.FromResult<PowerCard>(null);
		}

	}


}