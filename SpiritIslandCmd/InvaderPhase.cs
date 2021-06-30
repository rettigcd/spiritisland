using SpiritIsland;
using System;

namespace SpiritIslandCmd {
	class InvaderPhase : IPhase {

		public string Prompt => uiMap.ToPrompt();

		readonly GameState gameState;
		readonly InvaderDeck invaderDeck;

		public InvaderPhase(GameState gameState,InvaderDeck invaderDeck){
			this.gameState = gameState;
			this.invaderDeck = invaderDeck;
			uiMap = new UiMap("nothing to do", Array.Empty<IOption>(), null);
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

		public UiMap uiMap { get; set; }

		public void Select( IOption option ) {
			throw new NotImplementedException();
		}

		public event Action Complete;

	}

}
