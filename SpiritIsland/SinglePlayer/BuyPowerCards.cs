using System;
using System.Threading.Tasks;

namespace SpiritIsland.SinglePlayer {

	class BuyPowerCards : IPhase {

		readonly Spirit spirit;

		public event Action Complete;

		public BuyPowerCards( Spirit spirit ) {
			this.spirit = spirit;
		}

		public void Initialize() {
			_ = ActAndPublishComplete();
		}

		async Task ActAndPublishComplete() {
			await ActAsync();
			Complete?.Invoke();
		}


		public Task ActAsync() {
			return spirit.BuyPowerCardsAsync();
		}

	}

}
