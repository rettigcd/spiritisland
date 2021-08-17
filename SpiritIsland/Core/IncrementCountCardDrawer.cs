using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class IncrementCountCardDrawer : IPowerCardDrawer {
		public Task Draw( ActionEngine engine, Func<List<PowerCard>, Task> _ ) {
			engine.Self.PowerCardsToDraw++;
			return Task.CompletedTask;
		}

		public Task DrawMajor( ActionEngine engine, Func<List<PowerCard>, Task> _ ) {
			engine.Self.PowerCardsToDraw++;
			return Task.CompletedTask;
		}

		public Task DrawMinor( ActionEngine engine, Func<List<PowerCard>, Task> _ ) {
			engine.Self.PowerCardsToDraw++;
			return Task.CompletedTask;
		}

	}


}