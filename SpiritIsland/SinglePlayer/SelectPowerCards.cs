using System;
using System.Threading.Tasks;

namespace SpiritIsland.SinglePlayer {

	class SelectPowerCards : IPhase {

		readonly Spirit spirit;

		public event Action Complete;

		public SelectPowerCards( Spirit spirit ) {
			this.spirit = spirit;
		}

		public void Initialize() {
			_ = ActAndPublishComplete();
		}

		async Task ActAndPublishComplete() {
			await spirit.BuyPowerCardsAsync();
			Complete?.Invoke();
		}

	}

}
