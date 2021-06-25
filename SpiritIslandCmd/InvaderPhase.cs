using SpiritIsland;
using SpiritIsland.Invaders;
using System;

namespace SpiritIslandCmd {
	class InvaderPhase : IPhase {

//		readonly Spirit spirit;
		readonly GameState gameState;
		readonly InvaderDeck invaderDeck;

		public InvaderPhase(GameState gameState,InvaderDeck invaderDeck){
//			this.spirit = spirit;
			this.gameState = gameState;
			this.invaderDeck = invaderDeck;
		}

		public void Initialize() {
			gameState.Ravage(invaderDeck.Ravage);
			Console.WriteLine("Ravaging:" + invaderDeck.Ravage?.Text ?? "-");
			gameState.Build(invaderDeck.Build);
			Console.WriteLine("Building:" + invaderDeck.Build?.Text ?? "-");
			gameState.Explore(invaderDeck.Explore);
			Console.WriteLine("Exploring:" + invaderDeck.Explore?.Text ?? "-");
			invaderDeck.Advance();

			this.Complete?.Invoke();
		}

		public string Prompt => "nothing to do";

		public bool Handle( string cmd, int index ) => false;

		public event Action Complete;

	}

}
