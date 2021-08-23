using SpiritIsland;
using System;

namespace SpiritIsland.SinglePlayer {

	class TimePasses : IPhase {

		public IDecision Current => Decision.Null;

		readonly GameState gameState;
		public bool IsResolved => true;

		public TimePasses(GameState gameState){
			this.gameState = gameState;
		}

		public event Action Complete;

		public void Initialize() {
			_ = this.gameState.TimePasses();
			this.Complete?.Invoke();
		}

		public void Choose( IOption option ) {
			throw new NotImplementedException();
		}
	}

}
