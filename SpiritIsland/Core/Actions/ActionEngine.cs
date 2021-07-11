using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.Core {
	public class ActionEngine {
		public ActionEngine(Spirit self,GameState gameState){
			Self = self;
			GameState = gameState;
		}
		public Spirit Self { get; }
		public GameState GameState { get; }

		public readonly List<IAtomicAction> actions = new List<IAtomicAction>();
		public readonly Stack<IDecision> decisions = new Stack<IDecision>();

		public void Deconstruct(out Spirit self, out GameState gameState) {
	        self = Self;
			gameState = GameState;
		}

	}

}
