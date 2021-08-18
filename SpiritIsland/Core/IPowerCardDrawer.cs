using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritIsland {
	public interface IPowerCardDrawer {
		Task<PowerCard> Draw( Spirit spirit, GameState gameState, Func<List<PowerCard>,Task> handleNotUsed );
		Task<PowerCard> DrawMajor( Spirit spirit, GameState gameState, Func<List<PowerCard>, Task> handleNotUsed );
		Task<PowerCard> DrawMinor( Spirit spirit, GameState gameState, Func<List<PowerCard>, Task> handleNotUsed );
	}

}