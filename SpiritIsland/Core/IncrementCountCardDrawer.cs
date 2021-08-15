using System.Threading.Tasks;

namespace SpiritIsland {
	public class IncrementCountCardDrawer : IPowerCardDrawer {
		public Task Draw( ActionEngine engine ) {
			engine.Self.PowerCardsToDraw++;
			return Task.CompletedTask;
		}

		public Task DrawMajor( ActionEngine engine ) {
			engine.Self.PowerCardsToDraw++;
			return Task.CompletedTask;
		}

		public Task DrawMinor( ActionEngine engine ) {
			engine.Self.PowerCardsToDraw++;
			return Task.CompletedTask;
		}
	}

}