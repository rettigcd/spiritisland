using SpiritIsland;
using System;

namespace SpiritIsland.SinglePlayer {

	class TimePasses : IPhase {

		readonly GameState gameState;

		public TimePasses(GameState gameState){
			this.gameState = gameState;
		}

		public event Action Complete;

		public void Initialize() {
			_ = this.gameState.TimePasses();
			this.Complete?.Invoke();
		}

	}

}
