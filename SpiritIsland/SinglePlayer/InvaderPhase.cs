﻿using SpiritIsland;
using System;

namespace SpiritIslandCmd {

	class InvaderPhase : IPhase {

		public string Prompt => "nothing to do";
		public IOption[] Options => Array.Empty<IOption>();

		readonly GameState gameState;
		readonly InvaderDeck invaderDeck;

		public InvaderPhase(GameState gameState){
			this.gameState = gameState;
			this.invaderDeck = gameState.InvaderDeck;
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

		public void Select( IOption option ) {
			throw new NotImplementedException();
		}

		public event Action Complete;

	}

}
