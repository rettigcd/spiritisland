using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland.SinglePlayer {

	public class SelectGrowth {

		public SelectGrowth( Spirit spirit, GameState gameState ) {
			this.spirit = spirit;
			this.gameState = gameState;
		}

		public Task ActAsync() => spirit.DoGrowth(gameState);

		#region private
		readonly Spirit spirit;
		readonly GameState gameState;
		#endregion

	}

}
