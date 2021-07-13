using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Core {

	public class ActionEngine {

		public readonly Stack<IDecision> decisions = new Stack<IDecision>();
		public Spirit Self { get; }
		public GameState GameState { get; }

		public ActionEngine(Spirit self,GameState gameState){
			Self = self;
			GameState = gameState;
		}

		public void Deconstruct(out Spirit self, out GameState gameState) {
	        self = Self;
			gameState = GameState;
		}

	}

}
