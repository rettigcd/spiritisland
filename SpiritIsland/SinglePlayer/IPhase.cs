using System;
using SpiritIsland;

namespace SpiritIsland.SinglePlayer {

	interface IPhase : IDecision {
		void Initialize();
		event Action Complete;
	}

}
