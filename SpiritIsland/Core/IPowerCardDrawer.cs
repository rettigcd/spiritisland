using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland {
	public interface IPowerCardDrawer {
		Task<PowerCard> Draw( ActionEngine engine, Func<List<PowerCard>,Task> handleNotUsed );
		Task<PowerCard> DrawMajor( ActionEngine engine, Func<List<PowerCard>, Task> handleNotUsed );
		Task<PowerCard> DrawMinor( ActionEngine engine, Func<List<PowerCard>, Task> handleNotUsed );
	}

}