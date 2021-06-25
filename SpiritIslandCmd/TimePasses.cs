using SpiritIsland;
using System;

namespace SpiritIslandCmd {
	public class TimePasses : IPhase {
		
		readonly Spirit spirit;
//		readonly GameState gameState;

		public TimePasses(Spirit spirit,GameState _){
			this.spirit = spirit;
//			this.gameState = gameState;
		}

		public string Prompt => "nothing to do while time passes.";

		public event Action Complete;

		public bool Handle( string cmd, int index ) {
			return false;
		}

		public void Initialize() {
			// should this be in GameState.TimePasses???
			this.spirit.DiscardPile.AddRange(this.spirit.PurchasedCards);
			this.spirit.PurchasedCards.Clear();
			// !!! heal dahan & invaders
			this.Complete?.Invoke();
		}
	}

}
