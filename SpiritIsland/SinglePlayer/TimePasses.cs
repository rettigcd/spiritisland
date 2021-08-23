using SpiritIsland;
using System;

namespace SpiritIsland.SinglePlayer {

	class TimePasses : IPhase {

		public IDecision Current => Decision.Null;


		public string Prompt => Current.Prompt;
		public IOption[] Options => Current.Options;


		readonly GameState gameState;

		public TimePasses(GameState gameState){
			this.gameState = gameState;
		}

		public bool AllowAutoSelect { get; set; } = true;


		public event Action Complete;

		public void Initialize() {
			_ = this.gameState.TimePasses();
			this.Complete?.Invoke();
		}

		public void Select( IOption option ) {
			throw new NotImplementedException();
		}
	}

}
