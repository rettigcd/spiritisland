using System;
using System.Threading.Tasks;

namespace SpiritIsland.SinglePlayer {

	class TimePasses : IPhase {

		readonly GameState gameState;

		public TimePasses(GameState gameState){
			this.gameState = gameState;
		}

		public Task ActAsync() {
			_ = this.gameState.TimePasses();
			return Task.CompletedTask;
		}

		async Task ActAndTrigger() {
			await ActAsync();
			this.Complete?.Invoke();
		}

		public event Action Complete;

		public void Initialize() { _ = ActAndTrigger(); }

	}

}
