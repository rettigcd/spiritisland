using System;
using System.Threading.Tasks;

namespace SpiritIsland.SinglePlayer {

	class SelectPowerCards : IPhase {

		public IDecision GetCurrent() => spirit.Action.GetCurrent();
		public void Choose( IOption option ) => spirit.Action.Choose( option );
		public bool IsResolved => spirit.Action.IsResolved;

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
