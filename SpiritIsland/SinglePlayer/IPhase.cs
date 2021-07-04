using System;
using SpiritIsland.Core;

namespace SpiritIsland.SinglePlayer {

	interface IPhase : IDecision {
		void Initialize();
		event Action Complete;
	}

}
