﻿using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland.PowerCards {
	public class ActionEngine {
		public readonly List<IAtomicAction> actions = new List<IAtomicAction>();
		public readonly Stack<IDecision> decisions = new Stack<IDecision>();
	}

}
