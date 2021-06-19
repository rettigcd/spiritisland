using System.Collections.Generic;

namespace SpiritIsland.PowerCards {
	public class ActionEngine {
		public readonly List<IAtomicAction> moves = new List<IAtomicAction>();
		public readonly Stack<IDecision> decisions = new Stack<IDecision>();
	}

}
