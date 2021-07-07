using SpiritIsland;
using System;

namespace SpiritIsland.SinglePlayer {

	class TimePasses : IPhase {

		public string Prompt => "nothing to do while time passes.";

		readonly GameState gameState;

		public TimePasses(GameState gameState){
			this.gameState = gameState;
		}

		public IOption[] Options => Array.Empty<IOption>();

		public event Action Complete;

		public void Initialize() {
			this.gameState.TimePasses();

			// !!! heal dahan & invaders
			this.Complete?.Invoke();
		}

		public void Select( IOption option ) {
			throw new NotImplementedException();
		}
	}

}
