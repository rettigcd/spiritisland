using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland {
	public interface IPowerCardDrawer {
		Task Draw( ActionEngine engine, Func<List<PowerCard>,Task> handleNotUsed );
		Task DrawMajor( ActionEngine engine, Func<List<PowerCard>, Task> handleNotUsed );
		Task DrawMinor( ActionEngine engine, Func<List<PowerCard>, Task> handleNotUsed );
	}

}