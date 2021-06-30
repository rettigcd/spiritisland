using SpiritIsland;
using System;

namespace SpiritIslandCmd {

	class TimePasses : IPhase {

		public string Prompt => "nothing to do while time passes.";

		readonly Spirit spirit;

		public TimePasses(Spirit spirit,GameState _){
			this.spirit = spirit;
		}

		public IOption[] Options => Array.Empty<IOption>();

		public event Action Complete;

		public void Initialize() {
			// should this be in GameState.TimePasses???
			this.spirit.DiscardPile.AddRange(this.spirit.PurchasedCards);
			this.spirit.PurchasedCards.Clear();
			// !!! heal dahan & invaders
			this.Complete?.Invoke();
		}

		public void Select( IOption option ) {
			throw new NotImplementedException();
		}
	}

}
